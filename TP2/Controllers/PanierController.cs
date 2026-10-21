using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TP2.Models;
using TP2.Models.Repositories;
using TP2.Services;
using TP2.ViewModels;

namespace TP2.Controllers
{
    public class PanierController : Controller
    {
        private readonly IProductRepository productRepository;
        private readonly IOrderRepository orderRepository;
        private readonly UserManager<IdentityUser> userManager;
        private readonly ICartService cartService;
        private readonly AppDbContext context;

        public PanierController(
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            UserManager<IdentityUser> userManager,
            ICartService cartService,
            AppDbContext context)
        {
            this.productRepository = productRepository;
            this.orderRepository = orderRepository;
            this.userManager = userManager;
            this.cartService = cartService;
            this.context = context;
        }

        // GET: /Panier
        public async Task<IActionResult> Index()
        {
            var lines = await cartService.GetLinesAsync();
            var total = lines.Sum(l => l.LineTotal);
            ViewBag.Total = total;
            return View(lines);
        }

        // POST: /Panier/AddProduct — [HttpPost] + PRG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(int id)
        {
            var product = productRepository.GetById(id);
            if (product != null && product.QteStock > 0)
            {
                await cartService.AddAsync(id);
                TempData["ToastSuccess"] = $"« {product.Name} » ajouté au panier.";
            }
            else if (product != null)
            {
                TempData["ToastError"] = $"« {product.Name} » est en rupture de stock.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Panier/PlusProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlusProduct(int id)
        {
            var product = productRepository.GetById(id);
            if (product == null)
                return Json(new { success = false, message = "Produit introuvable." });

            var lines = await cartService.GetLinesAsync();
            var line = lines.FirstOrDefault(l => l.ProductId == id);
            int currentQty = line?.Quantity ?? 0;

            if (currentQty >= product.QteStock)
            {
                var total = lines.Sum(l => l.LineTotal);
                return Json(new
                {
                    success = false,
                    message = $"Stock max atteint ({product.QteStock} unités).",
                    Total = total.ToString("N2"),
                    CartCount = lines.Sum(l => l.Quantity)
                });
            }

            await cartService.AddAsync(id);
            var updatedLines = await cartService.GetLinesAsync();
            var updatedLine = updatedLines.FirstOrDefault(l => l.ProductId == id);
            var grandTotal = updatedLines.Sum(l => l.LineTotal);

            return Json(new
            {
                success = true,
                ct = 1,
                Total = grandTotal.ToString("N2"),
                Quatite = updatedLine?.Quantity ?? 0,
                TotalRow = updatedLine?.LineTotal.ToString("N2") ?? "0,00",
                CartCount = updatedLines.Sum(l => l.Quantity)
            });
        }

        // POST: /Panier/MinusProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MinusProduct(int id)
        {
            await cartService.DecreaseAsync(id);
            var lines = await cartService.GetLinesAsync();
            var line = lines.FirstOrDefault(l => l.ProductId == id);
            var grandTotal = lines.Sum(l => l.LineTotal);

            if (line != null)
            {
                return Json(new
                {
                    success = true,
                    ct = 1,
                    Total = grandTotal.ToString("N2"),
                    Quatite = line.Quantity,
                    TotalRow = line.LineTotal.ToString("N2"),
                    CartCount = lines.Sum(l => l.Quantity)
                });
            }

            // Ligne supprimée (quantité était 1)
            return Json(new
            {
                success = true,
                ct = 0,
                Total = grandTotal.ToString("N2"),
                CartCount = lines.Sum(l => l.Quantity)
            });
        }

        // POST: /Panier/RemoveProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProduct(int id)
        {
            await cartService.RemoveAsync(id);
            var lines = await cartService.GetLinesAsync();
            var grandTotal = lines.Sum(l => l.LineTotal);

            return Json(new
            {
                success = true,
                Total = grandTotal.ToString("N2"),
                CartCount = lines.Sum(l => l.Quantity)
            });
        }

        // GET: /Panier/CartCount  — pour le badge dans le header
        [HttpGet]
        public async Task<IActionResult> CartCount()
        {
            int count = await cartService.GetCountAsync();
            return Json(new { count });
        }

        // GET: /Panier/Checkout
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var lines = await cartService.GetLinesAsync();
            if (lines.Count == 0)
            {
                TempData["ToastError"] = "Votre panier est vide. Ajoutez des produits avant de commander.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new OrderViewModel
            {
                CartItems = lines,
                TotalAmount = lines.Sum(l => l.LineTotal)
            };

            return View(vm);
        }

        // POST: /Panier/Checkout
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderViewModel model)
        {
            var lines = await cartService.GetLinesAsync();

            if (lines.Count == 0)
            {
                TempData["ToastError"] = "Votre panier est vide.";
                return RedirectToAction(nameof(Index));
            }

            // Toujours recharger depuis la session (ne jamais faire confiance au formulaire)
            model.CartItems = lines;
            model.TotalAmount = lines.Sum(l => l.LineTotal);

            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Panier") });

            // Vérification du stock dans une transaction
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in lines)
                {
                    var product = productRepository.GetById(item.ProductId);
                    if (product == null)
                    {
                        ModelState.AddModelError("", $"Le produit « {item.ProductName} » n'existe plus.");
                        return View(model);
                    }
                    if (item.Quantity > product.QteStock)
                    {
                        ModelState.AddModelError("",
                            $"Stock insuffisant pour « {item.ProductName} » : {product.QteStock} disponible(s), {item.Quantity} demandé(s).");
                        return View(model);
                    }
                }

                // Décrémentation du stock
                foreach (var item in lines)
                {
                    var product = productRepository.GetById(item.ProductId)!;
                    product.QteStock -= item.Quantity;
                    productRepository.Update(product);
                }

                var order = new Order
                {
                    CustomerName = user.UserName ?? user.Email ?? "Client",
                    Email = user.Email ?? string.Empty,
                    Address = model.Address,
                    Phone = model.Phone,
                    PaymentMethod = model.PaymentMethod,
                    Status = "En attente",
                    TotalAmount = model.TotalAmount,
                    OrderDate = DateTime.UtcNow,
                    UserId = user.Id,
                    Items = lines.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }).ToList()
                };

                orderRepository.Add(order);
                await transaction.CommitAsync();
                cartService.Clear();

                TempData["SuccessMessage"] = "Votre commande a été passée avec succès !";
                return RedirectToAction(nameof(Confirmation), new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Une erreur est survenue : {ex.Message}");
                return View(model);
            }
        }

        // GET: /Panier/Confirmation?orderId=
        [Authorize]
        public async Task<IActionResult> Confirmation(int orderId)
        {
            var order = orderRepository.GetById(orderId);
            if (order == null)
                return NotFound();

            // IDOR protection
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Forbid();

            bool isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
            if (!isAdminOrManager && order.UserId != user.Id)
                return Forbid();

            return View(order);
        }

        // GET: /Panier/MesCommandes
        [Authorize]
        public async Task<IActionResult> MesCommandes()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var orders = orderRepository.GetByUserId(user.Id);
            return View(orders);
        }

        // GET: /Panier/OrderDetails/5
        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = orderRepository.GetById(id);
            if (order == null)
                return NotFound();

            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            bool isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
            if (!isAdminOrManager && order.UserId != user.Id)
                return Forbid();

            return View(order);
        }
    }
}
