using System.ComponentModel.DataAnnotations;
using TP2.Models;

namespace TP2.ViewModels
{
    /// <summary>
    /// ViewModel pour la création d'un produit avec upload d'image obligatoire.
    /// </summary>
    public class CreateViewModel
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 5)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Prix en dinar :")]
        public float Price { get; set; }

        [Required]
        [Display(Name = "Quantité en unité :")]
        public int QteStock { get; set; }

        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        // Image obligatoire à la création
        [Required]
        [Display(Name = "Image :")]
        public IFormFile ImagePath { get; set; } = null!;
    }
}
