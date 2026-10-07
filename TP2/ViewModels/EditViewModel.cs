using System.ComponentModel.DataAnnotations;
using TP2.Models;

namespace TP2.ViewModels
{
    /// <summary>
    /// ViewModel pour l'édition d'un produit.
    /// L'image n'est PAS obligatoire à l'édition : si l'utilisateur ne choisit pas
    /// de nouvelle image, l'ancienne (ExistingImagePath) est conservée.
    /// On ne redéclare pas ProductId (hérité de CreateViewModel).
    /// </summary>
    public class EditViewModel
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 5)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Prix en dinar :")]
        public float Price { get; set; }

        [Required]
        [Display(Name = "Quantité en unité :")]
        public int QteStock { get; set; }

        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        // Image optionnelle à l'édition (pas de [Required])
        [Display(Name = "Image :")]
        public IFormFile? ImagePath { get; set; }

        // Chemin de l'image existante (pour la conserver si pas de nouvelle image)
        public string? ExistingImagePath { get; set; }
    }
}
