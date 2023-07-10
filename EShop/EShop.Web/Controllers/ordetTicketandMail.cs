using EShop.Domain.DomainModels;
using EShop.Domain.DTO;
using EShop.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EShop.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            ILogger<OrderController> logger,
            IShoppingCartService shoppingCartService,
            IOrderService orderService)
        {
            _logger = logger;
            _shoppingCartService = shoppingCartService;
            _orderService = orderService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // Only authenticated users can create an order
        public async Task<IActionResult> CreateOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var shoppingCart = _shoppingCartService.GetShoppingCart(userId);
            if (shoppingCart == null || shoppingCart.Items.Count == 0)
            {
                return RedirectToAction("Index", "TicketSales");
            }

            // Create the order based on the items in the shopping cart
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = shoppingCart.GetTotalAmount(),
                Items = shoppingCart.Items
            };

            // Process the payment (example using Stripe)
            var paymentResult = await ProcessPayment(order);

            if (paymentResult)
            {
                // Save the order to the database
                _orderService.CreateOrder(order);

                // Clear the shopping cart after successful order creation
                _shoppingCartService.ClearShoppingCart(userId);

                // Send email notification to the user
                await SendEmailNotification(userId, order);

                _logger.LogInformation($"Order created for user '{userId}'. Order ID: '{order.Id}'");

                return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
            }

            _logger.LogError($"Payment failed for user '{userId}'. Order creation aborted.");
            return RedirectToAction("Index", "TicketSales");
        }

        [HttpGet]
        [Authorize]
        public IActionResult OrderConfirmation(Guid orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = _orderService.GetOrder(orderId, userId);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        private async Task<bool> ProcessPayment(Order order)
        {
            // Example payment processing using Stripe
            // Replace with your own payment processing logic

            // var stripeService = new StripeService();
            // var paymentResult = await stripeService.ProcessPayment(order);

            // return paymentResult;

            // For simplicity, returning a success result without actual payment processing
            return true;
        }

        private async Task SendEmailNotification(string userId, Order order)
        {
            // Example email notification sending logic
            // Replace with your own email sending logic

            // var emailService = new EmailService();
            // await emailService.SendOrderConfirmationEmail(userId, order);

            // For simplicity, logging the email notification instead
            _logger.LogInformation($"Email notification sent to user '{userId}' for order '{order.Id}'.");
        }
    }
}