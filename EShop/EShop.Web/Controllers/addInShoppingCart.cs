using EShop.Domain.DomainModels;
using EShop.Domain.DTO;
using EShop.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;

namespace EShop.Web.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(ILogger<ShoppingCartController> logger, IShoppingCartService shoppingCartService)
        {
            _logger = logger;
            _shoppingCartService = shoppingCartService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // Only authenticated users can add tickets to the shopping cart
        public IActionResult AddToCart(Guid ticketId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ticket = _shoppingCartService.GetTicketSale(ticketId);
            if (ticket == null)
            {
                return NotFound();
            }

            var result = _shoppingCartService.AddTicketSaleToCart(ticket, userId);

            if (result)
            {
                _logger.LogInformation($"User '{userId}' added ticket '{ticketId}' to the shopping cart.");
                return RedirectToAction("Index", "TicketSales");
            }

            return View(ticket);
        }
    }
}