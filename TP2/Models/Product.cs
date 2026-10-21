using System.ComponentModel.DataAnnotations;

namespace TP2.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Le nom doit avoir entre 2 et 50 caractères.")]
        [Display(Name = "Nom")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prix est obligatoire.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Le prix doit être supérieur à 0.")]
        [Display(Name = "Prix (DT)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "La quantité est obligatoire.")]
        [Range(0, int.MaxValue, ErrorMessage = "La quantité doit être ≥ 0.")]
        [Display(Name = "Quantité en stock")]
        public int QteStock { get; set; }

        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        [Display(Name = "Image")]
        public string Image { get; set; } = string.Empty;
    }
}