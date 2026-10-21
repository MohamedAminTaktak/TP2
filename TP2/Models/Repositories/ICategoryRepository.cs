namespace TP2.Models.Repositories
{
    public interface ICategoryRepository
    {
        Category? GetById(int Id);
        IList<Category> GetAll();
        /// <summary>Catégories avec le nombre de produits de chaque catégorie.</summary>
        IList<Category> GetAllWithProducts();
        void Add(Category t);
        Category? Update(Category t);
        void Delete(int Id);
        /// <summary>Retourne true si la catégorie contient des produits.</summary>
        bool HasProducts(int categoryId);
        int ProductCount(int categoryId);
    }
}
