using System.Collections.Generic;
using TP2.Models;

namespace TP2.ViewModels
{
    /// <summary>
    /// ViewModel principal pour la liste paginée + filtrée des produits.
    /// </summary>
    public class ProduitPaginationViewModel
    {
        public List<Product> Products { get; set; } = new();
        public int PageActuelle { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int? CategoryId { get; set; }
        public string? SearchTerm { get; set; }
        public string? CategoryName { get; set; }
        public string? SortBy { get; set; }
        public int PageSize { get; set; } = 4;

        public int FirstItem => TotalItems == 0 ? 0 : (PageActuelle - 1) * PageSize + 1;
        public int LastItem => Math.Min(PageActuelle * PageSize, TotalItems);
    }
}
