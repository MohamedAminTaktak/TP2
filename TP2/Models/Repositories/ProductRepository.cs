using Microsoft.EntityFrameworkCore;

namespace TP2.Models.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext context;

        public ProductRepository(AppDbContext context)
        {
            this.context = context;
        }

        public IList<Product> GetAll()
        {
            return context.Products
                .Include(x => x.Category)
                .OrderBy(x => x.Name)
                .ToList();
        }

        public Product? GetById(int id)
        {
            return context.Products
                .Include(x => x.Category)
                .SingleOrDefault(x => x.ProductId == id);
        }

        public void Add(Product p)
        {
            context.Products.Add(p);
            context.SaveChanges();
        }

        public IList<Product> FindByName(string name)
        {
            var searchTerm = name?.Trim();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return GetAll();
            }

            return context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.Contains(searchTerm) ||
                            (p.Category != null && p.Category.CategoryName.Contains(searchTerm)))
                .ToList();
        }

        public Product? Update(Product p)
        {
            Product? p1 = context.Products.Find(p.ProductId);
            if (p1 != null)
            {
                p1.Name = p.Name;
                p1.Price = p.Price;
                p1.QteStock = p.QteStock;
                p1.CategoryId = p.CategoryId;
                p1.Image = p.Image;
                context.SaveChanges();
            }
            return p1;
        }

        public void Delete(int productId)
        {
            Product? p1 = context.Products.Find(productId);
            if (p1 != null)
            {
                context.Products.Remove(p1);
                context.SaveChanges();
            }
        }

        public IList<Product> GetProductsByCategID(int? categId)
        {
            return context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categId)
                .OrderBy(p => p.ProductId)
                .ToList();
        }

        public IQueryable<Product> GetAllProducts()
        {
            return context.Products.Include(p => p.Category);
        }
    }
}
