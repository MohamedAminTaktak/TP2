using TP2.ViewModels;

namespace TP2.Services
{
    /// <summary>
    /// Service de panier stocké en session — une instance par utilisateur/session.
    /// </summary>
    public interface ICartService
    {
        /// <summary>Ajoute 1 unité du produit au panier.</summary>
        Task AddAsync(int productId);

        /// <summary>Retire 1 unité ; supprime la ligne si quantité atteint 0.</summary>
        Task DecreaseAsync(int productId);

        /// <summary>Supprime complètement une ligne du panier.</summary>
        Task RemoveAsync(int productId);

        /// <summary>Vide le panier.</summary>
        void Clear();

        /// <summary>
        /// Retourne les lignes du panier avec les infos produit rechargées depuis la BD.
        /// </summary>
        Task<List<CartItemViewModel>> GetLinesAsync();

        /// <summary>Somme des montants de toutes les lignes.</summary>
        Task<decimal> GetTotalAsync();

        /// <summary>Nombre total d'unités dans le panier (somme des quantités).</summary>
        Task<int> GetCountAsync();
    }
}
