using TP2.Models;

namespace TP2.Models.Repositories
{
    public interface IOrderRepository
    {
        void Add(Order o);
        Order? GetById(int id);
        IList<Order> GetByUserId(string userId);
        IList<Order> GetAll();
        void Update(Order o);
    }
}
