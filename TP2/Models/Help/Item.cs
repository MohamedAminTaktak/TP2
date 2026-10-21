using TP2.Models;

namespace TP2.Models.Help
{
    /// <summary>
    /// Représente une ligne du panier (un produit + sa quantité).
    /// </summary>
    public class Item
    {
        public int quantite { get; set; }
        private Product _product;

        public Product Prod
        {
            get => _product;
            set => _product = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Description => _product.Name;

        public decimal UnitPrice => _product.Price;

        public int CategoryId => _product.CategoryId;

        public Category? Category => _product.Category;

        public decimal TotalPrice => _product.Price * quantite;

        public Item(Product p)
        {
            _product = p ?? throw new ArgumentNullException(nameof(p));
        }

        public bool Equals(Item item)
        {
            return item.Prod.ProductId == this.Prod.ProductId;
        }
    }
}
