using System.ComponentModel.DataAnnotations;

namespace TP2.ViewModels
{
    /// <summary>
    /// ViewModel pour l'édition d'un produit.
    /// L'image n'est PAS obligatoire à l'édition : si l'utilisateur ne choisit pas
    /// de nouvelle image, l'ancienne (ExistingImagePath) est conservée.
    /// </summary>
    public class EditViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Le nom doit avoir entre 2 et 50 caractères.")]
        [Display(Name = "Nom du produit")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prix est obligatoire.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le prix doit être supérieur à 0.")]
        [Display(Name = "Prix (DT)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "La quantité est obligatoire.")]
        [Range(0, int.MaxValue, ErrorMessage = "La quantité doit être ≥ 0.")]
        [Display(Name = "Quantité en stock")]
        public int QteStock { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Veuillez choisir une catégorie.")]
        [Display(Name = "Catégorie")]
        public int CategoryId { get; set; }

        // Image optionnelle à l'édition (pas de [Required])
        [Display(Name = "Nouvelle image (optionnel)")]
        public IFormFile? ImagePath { get; set; }

        // Chemin de l'image existante (pour la conserver si pas de nouvelle image)
        public string? ExistingImagePath { get; set; }
    }
}
