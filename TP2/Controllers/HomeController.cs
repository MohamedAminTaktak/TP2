using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TP2.Models;
using TP2.Models.Repositories;

namespace TP2.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository productRepository;
        private readonly ICategoryRepository categoryRepository;

        public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            this.productRepository = productRepository;
            this.categoryRepository = categoryRepository;
        }

        public IActionResult Index()
        {
            // Catégories avec nombre de produits
            var categories = categoryRepository.GetAllWithProducts();

            // 8 derniers produits
            var latestProducts = productRepository.GetAllProducts()
                .OrderByDescending(p => p.ProductId)
                .Take(8)
                .ToList();

            ViewBag.Categories = categories;
            ViewBag.LatestProducts = latestProducts;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("Home/HandleError/{code}")]
        public IActionResult HandleError(int code)
        {
            if (code == 404)
            {
                Response.StatusCode = 404;
                return View("~/Views/Shared/NotFound.cshtml");
            }

            if (code == 403)
            {
                Response.StatusCode = 403;
                return View("~/Views/Account/AccessDenied.cshtml");
            }

            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
