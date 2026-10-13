using System.Linq;
using Microsoft.EntityFrameworkCore;
using TP2.Models;

namespace TP2.Models.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        readonly AppDbContext context;

        public OrderRepository(AppDbContext context)
        {
            this.context = context;
        }

        public void Add(Order o)
        {
            context.Orders.Add(o);
            context.SaveChanges();
        }

        public Order? GetById(int id)
        {
            return context.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.Id == id);
        }
    }
}
