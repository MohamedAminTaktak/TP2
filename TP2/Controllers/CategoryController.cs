using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        // GET: CategoryController
        [AllowAnonymous]
        public ActionResult Index()
        {
            var Categories = CategRepository.GetAll();

            return View(Categories);
        }

        // GET: CategoryController/Details/5
        public ActionResult Details(int id)
        {
            var category = CategRepository.GetById(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: CategoryController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CategoryController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CategRepository.Add(category);

                    return RedirectToAction(nameof(Index));
                }

                return View(category);
            }
            catch
            {
                return View(category);
            }
        }

        // GET: CategoryController/Edit/5
        public ActionResult Edit(int id)
        {
            var category = CategRepository.GetById(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: CategoryController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Category category)
        {
            try
            {
                if (id != category.CategoryId)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    var updatedCategory = CategRepository.Update(category);

                    if (updatedCategory == null)
                    {
                        return NotFound();
                    }

                    return RedirectToAction(nameof(Index));
                }

                return View(category);
            }
            catch
            {
                return View(category);
            }
        }

        // GET: CategoryController/Delete/5
        public ActionResult Delete(int id)
        {
            var category = CategRepository.GetById(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: CategoryController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                var category = CategRepository.GetById(id);

                if (category == null)
                {
                    return NotFound();
                }

                CategRepository.Delete(id);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}