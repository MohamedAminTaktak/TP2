using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TP2.Models;
using TP2.Models.Repositories;
using TP2.ViewModels;

namespace TP2.Controllers
{
    public class ProductController : Controller
    {
        readonly IProductRepository ProductRepository;
        readonly ICategoryRepository CategRepository;
        // Permet d'accéder au chemin physique de wwwroot
        private readonly IWebHostEnvironment hostingEnvironment;

        public ProductController(
            IProductRepository ProdRepository,
            ICategoryRepository categRepository,
            IWebHostEnvironment hostingEnvironment)
        {
            ProductRepository = ProdRepository;
            CategRepository = categRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: ProductController
        public ActionResult Index()
        {
            var Products = ProductRepository.GetAll();
            return View(Products);
        }

        // GET: ProductController/Details/5
        public ActionResult Details(int id)
        {
            var product = ProductRepository.GetById(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: ProductController/Create
        public ActionResult Create()
        {
            // Charger les catégories depuis la base de données
            ViewBag.CategoryId = new SelectList(
                CategRepository.GetAll(),
                "CategoryId",
                "CategoryName"
            );

            return View();
        }

        // POST: ProductController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Enregistrer l'image uploadée et récupérer le nom du fichier
                string uniqueFileName = ProcessUploadedFile(model.ImagePath);

                // Créer le produit avec le nom du fichier image
                Product product = new Product
                {
                    Name = model.Name,
                    Price = model.Price,
                    QteStock = model.QteStock,
                    CategoryId = model.CategoryId,
                    Image = uniqueFileName
                };

                ProductRepository.Add(product);
                return RedirectToAction("Details", new { id = product.ProductId });
            }

            // Recharger les catégories si le modèle est invalide
            ViewBag.CategoryId = new SelectList(
                CategRepository.GetAll(),
                "CategoryId",
                "CategoryName"
            );

            return View(model);
        }

        // GET: ProductController/Edit/5
        public ActionResult Edit(int id)
        {
            var product = ProductRepository.GetById(id);

            if (product == null)
            {
                return NotFound();
            }

            // Charger les catégories
            ViewBag.CategoryId = new SelectList(
                CategRepository.GetAll(),
                "CategoryId",
                "CategoryName",
                product.CategoryId
            );

            // Mapper le produit vers le EditViewModel
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

        // POST: ProductController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditViewModel model)
        {
            // Recharger les catégories dans tous les cas
            ViewBag.CategoryId = new SelectList(
                CategRepository.GetAll(),
                "CategoryId",
                "CategoryName",
                model.CategoryId
            );

            if (ModelState.IsValid)
            {
                var product = ProductRepository.GetById(model.ProductId);
                if (product == null)
                {
                    return NotFound();
                }

                // Mettre à jour les propriétés du produit
                product.Name = model.Name;
                product.Price = model.Price;
                product.QteStock = model.QteStock;
                product.CategoryId = model.CategoryId;

                // Si une nouvelle image est envoyée
                if (model.ImagePath != null)
                {
                    // Supprimer l'ancienne image du disque si elle existe
                    if (!string.IsNullOrEmpty(model.ExistingImagePath))
                    {
                        string oldFilePath = Path.Combine(
                            hostingEnvironment.WebRootPath, "images", model.ExistingImagePath);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Enregistrer la nouvelle image
                    product.Image = ProcessUploadedFile(model.ImagePath);
                }

                ProductRepository.Update(product);
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: ProductController/Delete/5
        public ActionResult Delete(int id)
        {
            var product = ProductRepository.GetById(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: ProductController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                var product = ProductRepository.GetById(id);

                if (product == null)
                {
                    return NotFound();
                }

                ProductRepository.Delete(id);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // Recherche par nom de produit ou nom de catégorie
        public ActionResult Search(string val)
        {
            var result = ProductRepository.FindByName(val);
            return View("Index", result);
        }

        /// <summary>
        /// Méthode privée pour traiter l'upload d'un fichier image.
        /// Génère un nom unique (GUID) et enregistre le fichier dans wwwroot/images.
        /// </summary>
        [NonAction]
        private string ProcessUploadedFile(IFormFile file)
        {
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");

            // Créer le dossier images s'il n'existe pas
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Utiliser un using pour libérer le FileStream
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return uniqueFileName;
        }
    }
}