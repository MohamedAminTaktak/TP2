using System.Collections.Generic;
using TP2.Models;

namespace TP2.ViewModels
{
    public class ProduitPaginationViewModel
    {
        public List<Product> Products { get; set; } = new();
        public int PageActuelle { get; set; }
        public int TotalPages { get; set; }
    }
}
