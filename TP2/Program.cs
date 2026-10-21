using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using TP2.Models;
using TP2.Models.Repositories;
using TP2.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ───────────────────────────────────────────────────────────────

builder.Services.AddControllersWithViews();

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDBConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Session-based cart (per user)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICartService, CartService>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ── Build ──────────────────────────────────────────────────────────────────

var app = builder.Build();

// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAndAdminAsync(services);
}

// ── Middleware pipeline ────────────────────────────────────────────────────

app.UseStatusCodePagesWithReExecute("/Home/HandleError/{0}");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Localisation : format des nombres en français (virgule décimale, DT)
var frCulture = new CultureInfo("fr-FR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(frCulture),
    SupportedCultures = new[] { frCulture },
    SupportedUICultures = new[] { frCulture }
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Session DOIT être avant UseAuthentication
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// ── Seeder ─────────────────────────────────────────────────────────────────

static async Task SeedRolesAndAdminAsync(IServiceProvider services)
{
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var configuration = services.GetRequiredService<IConfiguration>();

    string[] roles = ["Admin", "Manager"];

    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Impossible de créer le rôle '{roleName}' : {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    // Credentials from appsettings.Development.json or User Secrets
    var adminSection = configuration.GetSection("AdminUser");
    var adminEmail = adminSection["Email"] ?? "admin@tp2.com";
    var adminPassword = adminSection["Password"] ?? "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(
                $"Impossible de créer l'utilisateur admin : {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
    }

    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
        if (!roleResult.Succeeded)
            throw new InvalidOperationException(
                $"Impossible d'assigner le rôle Admin : {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
    }

    logger.LogInformation("Rôles Identity et utilisateur admin initialisés avec succès.");
}