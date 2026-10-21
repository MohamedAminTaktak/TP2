using Microsoft.AspNetCore.Mvc;
using TP2.Services;

namespace TP2.ViewComponents
{
    public class CartBadgeViewComponent : ViewComponent
    {
        private readonly ICartService cartService;

        public CartBadgeViewComponent(ICartService cartService)
        {
            this.cartService = cartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int count = await cartService.GetCountAsync();
            return View(count);
        }
    }
}
