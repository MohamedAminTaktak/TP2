using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TP2.Models;
using TP2.Models.Repositories;
using TP2.ViewModels;

namespace TP2.Services
{
    /// <summary>
    /// Panier sérialisé en JSON dans la session ASP.NET Core.
    /// Stocke seulement (ProductId, Quantity) ; recharge prix/nom/image depuis la BD.
    /// </summary>
    public class CartService : ICartService
    {
        private const string SessionKey = "Cart";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductRepository _productRepository;

        public CartService(IHttpContextAccessor httpContextAccessor, IProductRepository productRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _productRepository = productRepository;
        }

        // ── Session helpers ────────────────────────────────────────────────────

        private ISession Session =>
            _httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("Session non disponible.");

        private List<CartLine> GetRawLines()
        {
            var json = Session.GetString(SessionKey);
            return json == null
                ? new List<CartLine>()
                : JsonSerializer.Deserialize<List<CartLine>>(json) ?? new List<CartLine>();
        }

        private void SaveRawLines(List<CartLine> lines)
        {
            Session.SetString(SessionKey, JsonSerializer.Serialize(lines));
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public Task AddAsync(int productId)
        {
            var lines = GetRawLines();
            var existing = lines.Find(l => l.ProductId == productId);
            if (existing != null)
                existing.Quantity++;
            else
                lines.Add(new CartLine { ProductId = productId, Quantity = 1 });
            SaveRawLines(lines);
            return Task.CompletedTask;
        }

        public Task DecreaseAsync(int productId)
        {
            var lines = GetRawLines();
            var existing = lines.Find(l => l.ProductId == productId);
            if (existing != null)
            {
                if (existing.Quantity <= 1)
                    lines.Remove(existing);
                else
                    existing.Quantity--;
            }
            SaveRawLines(lines);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(int productId)
        {
            var lines = GetRawLines();
            lines.RemoveAll(l => l.ProductId == productId);
            SaveRawLines(lines);
            return Task.CompletedTask;
        }

        public void Clear()
        {
            Session.Remove(SessionKey);
        }

        public async Task<List<CartItemViewModel>> GetLinesAsync()
        {
            var raw = GetRawLines();
            if (raw.Count == 0)
                return new List<CartItemViewModel>();

            var ids = raw.Select(l => l.ProductId).ToList();
            var products = _productRepository.GetAll()
                .Where(p => ids.Contains(p.ProductId))
                .ToList();

            var result = new List<CartItemViewModel>();
            foreach (var line in raw)
            {
                var product = products.FirstOrDefault(p => p.ProductId == line.ProductId);
                if (product == null) continue; // produit supprimé entre-temps

                result.Add(new CartItemViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Quantity = line.Quantity,
                    Price = product.Price,           // prix toujours frais depuis la BD
                    Image = product.Image,
                    CategoryName = product.Category?.CategoryName
                });
            }
            return await Task.FromResult(result);
        }

        public async Task<decimal> GetTotalAsync()
        {
            var lines = await GetLinesAsync();
            return lines.Sum(l => l.LineTotal);
        }

        public async Task<int> GetCountAsync()
        {
            var raw = GetRawLines();
            return await Task.FromResult(raw.Sum(l => l.Quantity));
        }

        // ── Inner DTO (JSON-serialisable, minimal) ────────────────────────────

        private class CartLine
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
