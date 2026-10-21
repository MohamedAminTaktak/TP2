using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TP2.Models;
using TP2.Models.Repositories;
using TP2.Services;
using TP2.ViewModels;

namespace TP2.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ProductController : Controller
    {
        private readonly IProductRepository ProductRepository;
        private readonly ICategoryRepository CategRepository;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly ICartService cartService;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxImageSize = 2 * 1024 * 1024; // 2 MB

        public ProductController(
            IProductRepository prodRepository,
            ICategoryRepository categRepository,
            IWebHostEnvironment hostingEnvironment,
            ICartService cartService)
        {
            ProductRepository = prodRepository;
            CategRepository = categRepository;
            this.hostingEnvironment = hostingEnvironment;
            this.cartService = cartService;
        }

        // GET: /Product/Index?categoryId=&search=&sortBy=&page=
        [AllowAnonymous]
        public IActionResult Index(int? categoryId, string? search, string? sortBy, int page = 1)
        {
            const int pageSize = 4;

            IQueryable<Product> query = ProductRepository.GetAllProducts();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    (p.Category != null && p.Category.CategoryName.Contains(search)));

            // Tri
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "recent" => query.OrderByDescending(p => p.ProductId),
                _ => query.OrderBy(p => p.Name)
            };

            int total = query.Count();
            int totalPages = total == 0 ? 1 : (int)Math.Ceiling((double)total / pageSize);
            page = Math.Clamp(page, 1, totalPages);

            var products = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            string? catName = categoryId.HasValue
                ? CategRepository.GetById(categoryId.Value)?.CategoryName
                : null;

            var vm = new ProduitPaginationViewModel
            {
                Products = products,
                PageActuelle = page,
                TotalPages = totalPages,
                TotalItems = total,
                CategoryId = categoryId,
                SearchTerm = search,
                CategoryName = catName,
                SortBy = sortBy,
                PageSize = pageSize
            };

            return View(vm);
        }

        // GET: /Product/Details/5
        [AllowAnonymous]
        public ActionResult Details(int id)
        {
            var product = ProductRepository.GetById(id);
            if (product == null)
                return NotFound();

            var similar = ProductRepository.GetAllProducts()
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != product.ProductId)
                .Take(4)
                .ToList();
            ViewBag.SimilarProducts = similar;

            return View(product);
        }

        // POST: /Product/AddToCart  (AJAX ou POST normal)
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? returnUrl = null)
        {
            var product = ProductRepository.GetById(productId);
            if (product == null)
                return NotFound();

            if (quantity < 1) quantity = 1;
            for (int i = 0; i < quantity; i++)
            {
                await cartService.AddAsync(productId);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                int count = await cartService.GetCountAsync();
                return Json(new { success = true, cartCount = count, message = $"« {product.Name} » a été ajouté au panier." });
            }

            TempData["ToastSuccess"] = $"« {product.Name} » a été ajouté au panier.";
            return Redirect(returnUrl ?? Url.Action("Index", "Product")!);
        }

        // GET: /Product/Create
        public ActionResult Create()
        {
            LoadCategorySelectList();
            return View(new CreateViewModel());
        }

        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var (ok, fileName, error) = ProcessUploadedFile(model.ImagePath);
                if (!ok)
                {
                    ModelState.AddModelError("ImagePath", error!);
                    LoadCategorySelectList(model.CategoryId);
                    return View(model);
                }

                Product product = new Product
                {
                    Name = model.Name,
                    Price = model.Price,
                    QteStock = model.QteStock,
                    CategoryId = model.CategoryId,
                    Image = fileName!
                };

                ProductRepository.Add(product);
                TempData["ToastSuccess"] = $"Produit « {product.Name} » créé avec succès.";
                return RedirectToAction(nameof(Details), new { id = product.ProductId });
            }

            LoadCategorySelectList(model.CategoryId);
            return View(model);
        }

        // GET: /Product/Edit/5
        public ActionResult Edit(int id)
        {
            var product = ProductRepository.GetById(id);
            if (product == null)
                return NotFound();

            LoadCategorySelectList(product.CategoryId);

            EditViewModel model = new EditViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                QteStock = product.QteStock,
                CategoryId = product.CategoryId,
                ExistingImagePath = product.Image
            };

            return View(model);
        }

        // POST: /Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditViewModel model)
        {
            LoadCategorySelectList(model.CategoryId);

            if (ModelState.IsValid)
            {
                var product = ProductRepository.GetById(model.ProductId);
                if (product == null)
                    return NotFound();

                product.Name = model.Name;
                product.Price = model.Price;
                product.QteStock = model.QteStock;
                product.CategoryId = model.CategoryId;

                if (model.ImagePath != null)
                {
                    var (ok, fileName, error) = ProcessUploadedFile(model.ImagePath);
                    if (!ok)
                    {
                        ModelState.AddModelError("ImagePath", error!);
                        return View(model);
                    }

                    // Supprimer l'ancienne image
                    DeleteImageFile(model.ExistingImagePath);
                    product.Image = fileName!;
                }

                ProductRepository.Update(product);
                TempData["ToastSuccess"] = $"Produit « {product.Name} » mis à jour.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: /Product/Delete/5
        public ActionResult Delete(int id)
        {
            var product = ProductRepository.GetById(id);
            if (product == null)
                return NotFound();
            return View(product);
        }

        // POST: /Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var product = ProductRepository.GetById(id);
                if (product == null)
                    return NotFound();

                string? imagePath = product.Image;
                ProductRepository.Delete(id);
                DeleteImageFile(imagePath);

                TempData["ToastSuccess"] = "Produit supprimé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ToastError"] = $"Erreur lors de la suppression : {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void LoadCategorySelectList(int? selectedId = null)
        {
            ViewBag.CategoryId = new SelectList(
                CategRepository.GetAll(),
                "CategoryId",
                "CategoryName",
                selectedId);
        }

        [NonAction]
        private (bool ok, string? fileName, string? error) ProcessUploadedFile(IFormFile? file)
        {
            if (file == null)
                return (false, null, "Aucun fichier sélectionné.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return (false, null, $"Extension non autorisée. Formats acceptés : {string.Join(", ", AllowedExtensions)}");

            if (file.Length > MaxImageSize)
                return (false, null, "L'image ne doit pas dépasser 2 Mo.");

            string safeName = Path.GetFileName(file.FileName);
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + safeName;
            string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images", "products");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(fileStream);

            return (true, "products/" + uniqueFileName, null);
        }

        [NonAction]
        private void DeleteImageFile(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            string fullPath = Path.Combine(hostingEnvironment.WebRootPath, "images", imagePath);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }
}