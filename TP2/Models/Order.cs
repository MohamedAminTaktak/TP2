using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace TP2.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = "En attente";
        // Lien avec l'utilisateur dans Identity
        public string UserId { get; set; } = string.Empty;
        public IdentityUser? User { get; set; }
        // Liste des articles de la commande
        public List<OrderItem> Items { get; set; } = new();
    }
}
