using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TP2.Models;
using TP2.Models.Repositories;

namespace TP2.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class CategoryController : Controller
    {
        readonly ICategoryRepository CategRepository;

        public CategoryController(ICategoryRepository categRepository)
        {
            CategRepository = categRepository;
        }

        // GET: /Category
        [AllowAnonymous]
        public ActionResult Index()
        {
            var categories = CategRepository.GetAllWithProducts();
            return View(categories);
        }

        // GET: /Category/Details/5
        public ActionResult Details(int id)
        {
            var category = CategRepository.GetById(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        // GET: /Category/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CategRepository.Add(category);
                    TempData["ToastSuccess"] = $"Catégorie « {category.CategoryName} » créée.";
                    return RedirectToAction(nameof(Index));
                }
                return View(category);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erreur : {ex.Message}");
                return View(category);
            }
        }

        // GET: /Category/Edit/5
        public ActionResult Edit(int id)
        {
            var category = CategRepository.GetById(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        // POST: /Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Category category)
        {
            try
            {
                if (id != category.CategoryId)
                    return NotFound();

                if (ModelState.IsValid)
                {
                    var updated = CategRepository.Update(category);
                    if (updated == null)
                        return NotFound();
                    TempData["ToastSuccess"] = "Catégorie mise à jour.";
                    return RedirectToAction(nameof(Index));
                }
                return View(category);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erreur : {ex.Message}");
                return View(category);
            }
        }

        // GET: /Category/Delete/5
        public ActionResult Delete(int id)
        {
            var category = CategRepository.GetById(id);
            if (category == null)
                return NotFound();

            int count = CategRepository.ProductCount(id);
            ViewBag.ProductCount = count;
            return View(category);
        }

        // POST: /Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var category = CategRepository.GetById(id);
            if (category == null)
                return NotFound();

            if (CategRepository.HasProducts(id))
            {
                int count = CategRepository.ProductCount(id);
                TempData["ToastError"] = $"Impossible de supprimer « {category.CategoryName} » : elle contient {count} produit(s). Supprimez d'abord les produits.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                CategRepository.Delete(id);
                TempData["ToastSuccess"] = $"Catégorie « {category.CategoryName} » supprimée.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ToastError"] = $"Erreur lors de la suppression : {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}