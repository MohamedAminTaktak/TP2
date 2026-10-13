using TP2.Models;

namespace TP2.Models.Repositories
{
    public interface IOrderRepository
    {
        Order GetById(int Id);
        void Add(Order o);
    }
}
