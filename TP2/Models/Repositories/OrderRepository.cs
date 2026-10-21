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

        public IList<Order> GetByUserId(string userId)
        {
            return context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public IList<Order> GetAll()
        {
            return context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public void Update(Order o)
        {
            context.Orders.Update(o);
            context.SaveChanges();
        }
    }
}
