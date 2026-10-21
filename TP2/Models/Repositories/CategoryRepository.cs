using Microsoft.EntityFrameworkCore;

namespace TP2.Models.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext context;

        public CategoryRepository(AppDbContext context)
        {
            this.context = context;
        }

        public IList<Category> GetAll()
        {
            return context.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();
        }

        public IList<Category> GetAllWithProducts()
        {
            return context.Categories
                .Include(c => c.Products)
                .OrderBy(c => c.CategoryName)
                .ToList();
        }

        public Category? GetById(int id)
        {
            return context.Categories.Find(id);
        }

        public bool HasProducts(int categoryId)
        {
            return context.Products.Any(p => p.CategoryId == categoryId);
        }

        public int ProductCount(int categoryId)
        {
            return context.Products.Count(p => p.CategoryId == categoryId);
        }

        public void Add(Category c)
        {
            context.Categories.Add(c);
            context.SaveChanges();
        }

        public Category? Update(Category c)
        {
            Category? c1 = context.Categories.Find(c.CategoryId);
            if (c1 != null)
            {
                c1.CategoryName = c.CategoryName;
                context.SaveChanges();
            }
            return c1;
        }

        public void Delete(int categoryId)
        {
            Category? c1 = context.Categories.Find(categoryId);
            if (c1 != null)
            {
                context.Categories.Remove(c1);
                context.SaveChanges();
            }
        }
    }
}
