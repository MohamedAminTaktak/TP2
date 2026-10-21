using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TP2.ViewModels
{
    public class OrderViewModel
    {
        [Required(ErrorMessage = "L'adresse de livraison est obligatoire.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "L'adresse doit avoir entre 5 et 500 caractères.")]
        [Display(Name = "Adresse de livraison")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Téléphone (optionnel)")]
        [Phone(ErrorMessage = "Numéro de téléphone invalide.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Veuillez choisir une méthode de paiement.")]
        [Display(Name = "Méthode de paiement")]
        public string PaymentMethod { get; set; } = string.Empty;

        // Calculé côté serveur — ne jamais provenir du formulaire
        public decimal TotalAmount { get; set; }
        public List<CartItemViewModel> CartItems { get; set; } = new();
    }
}
