using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EShop.Service.Interface;

namespace EShop.Web.Controllers
{
    public class TicketSalesController : Controller
    {
        private readonly ITicketSaleService _ticketSaleService;

        public TicketSalesController(ITicketSaleService ticketSaleService)
        {
            _ticketSaleService = ticketSaleService;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(_ticketSaleService.GetTicketSalesForUser(userId));
        }

        public IActionResult AddToCart(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = _ticketSaleService.AddTicketSaleToCart(id, userId);
            if (result)
            {
                return RedirectToAction("Index", "TicketSales");
            }
            else
            {
                return RedirectToAction("Index", "TicketSales");
            }
        }

        public IActionResult RemoveFromCart(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = _ticketSaleService.RemoveTicketSaleFromCart(id, userId);
            if (result)
            {
                return RedirectToAction("Index", "TicketSales");
            }
            else
            {
                return RedirectToAction("Index", "TicketSales");
            }
        }

        public IActionResult PlaceOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = _ticketSaleService.PlaceOrder(userId);
            if (result)
            {
                return RedirectToAction("Index", "TicketSales");
            }
            else
            {
                return RedirectToAction("Index", "TicketSales");
            }
        }

        public IActionResult ProcessPayment(string stripeEmail, string stripeToken)
        {
            var customerService = new CustomerService();
            var chargeService = new ChargeService();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ticketSales = _ticketSaleService.GetTicketSalesInCart(userId);
            var totalPrice = ticketSales.Sum(ts => ts.Price);

            var customer = customerService.Create(new CustomerCreateOptions
            {
                Email = stripeEmail,
                Source = stripeToken
            });

            var charge = chargeService.Create(new ChargeCreateOptions
            {
                Amount = (Convert.ToInt32(totalPrice) * 100),
                Description = "Ticket Sale Payment",
                Currency = "usd",
                Customer = customer.Id
            });

            if (charge.Status == "succeeded")
            {
                var result = _ticketSaleService.CompleteOrder(userId);
                if (result)
                {
                    return RedirectToAction("Index", "TicketSales");
                }
                else
                {
                    return RedirectToAction("Index", "TicketSales");
                }
            }

            return RedirectToAction("Index", "TicketSales");
        }
    }
}