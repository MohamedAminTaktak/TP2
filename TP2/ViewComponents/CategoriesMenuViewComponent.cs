using Microsoft.AspNetCore.Mvc;
using TP2.Models.Repositories;

namespace TP2.ViewComponents
{
    /// <summary>
    /// ViewComponent pour le menu latéral des catégories avec nombre de produits.
    /// Remplace le partial _CategoriesMenu qui dépendait de ViewData.
    /// </summary>
    public class CategoriesMenuViewComponent : ViewComponent
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesMenuViewComponent(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public IViewComponentResult Invoke()
        {
            var categories = _categoryRepository.GetAllWithProducts();
            // Catégorie active depuis la route
            var activeId = HttpContext.Request.Query["categoryId"].FirstOrDefault();
            int.TryParse(activeId, out int activeCategoryId);
            ViewBag.ActiveCategoryId = activeCategoryId;
            return View(categories);
        }
    }
}
