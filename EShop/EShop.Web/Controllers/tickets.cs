using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EShop.Domain.DomainModels;
using EShop.Domain.DTO;
using EShop.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Web.Controllers
{
    public class TicketSalesController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<TicketSalesController> _logger;

        public TicketSalesController(ILogger<TicketSalesController> logger, IProductService productService)
        {
            _logger = logger;
            _productService = productService;
        }

        // GET: TicketSales
        public IActionResult Index()
        {
            _logger.LogInformation("User Request -> Get All ticket sales!");
            return View(_productService.GetAllTicketSales());
        }

        // GET: TicketSales/Details/5
        public IActionResult Details(Guid? id)
        {
            _logger.LogInformation("User Request -> Get Details For Ticket Sale");
            if (id == null)
            {
                return NotFound();
            }

            var ticketSale = _productService.GetDetailsForTicketSale(id);
            if (ticketSale == null)
            {
                return NotFound();
            }

            return View(ticketSale);
        }

        // GET: TicketSales/Create
        public IActionResult Create()
        {
            _logger.LogInformation("User Request -> Get create form for Ticket Sale!");
            return View();
        }

        // POST: TicketSales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,TicketName,TicketImage,TicketDescription,TicketPrice")] TicketSale ticketSale)
        {
            _logger.LogInformation("User Request -> Insert Ticket Sale in Database!");
            if (ModelState.IsValid)
            {
                ticketSale.Id = Guid.NewGuid();
                _productService.CreateNewTicketSale(ticketSale);
                return RedirectToAction(nameof(Index));
            }
            return View(ticketSale);
        }

        // GET: TicketSales/Edit/5
        public IActionResult Edit(Guid? id)
        {
            _logger.LogInformation("User Request -> Get edit form for Ticket Sale!");
            if (id == null)
            {
                return NotFound();
            }

            var ticketSale = _productService.GetDetailsForTicketSale(id);
            if (ticketSale == null)
            {
                return NotFound();
            }
            return View(ticketSale);
        }

        // POST: TicketSales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, [Bind("Id,TicketName,TicketImage,TicketDescription,TicketPrice")] TicketSale ticketSale)
        {
            _logger.LogInformation("User Request -> Update Ticket Sale in Database!");

            if (id != ticketSale.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _productService.UpdateExistingTicketSale(ticketSale);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketSaleExists(ticketSale.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ticketSale);
        }

        // GET: TicketSales/Delete/5
        public IActionResult Delete(Guid? id)
        {
            _logger.LogInformation("User Request -> Get delete form for Ticket Sale!");

            if (id == null)
            {
                return NotFound();
            }

            var ticketSale = _productService.GetDetailsForTicketSale(id);
            if (ticketSale == null)
            {
                return NotFound();
            }

            return View(ticketSale);
        }

        // POST: TicketSales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _logger.LogInformation("User Request -> Delete Ticket Sale in Database!");

            _productService.DeleteTicketSale(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult AddToCart(Guid id)
        {
            var ticketSale = _productService.GetDetailsForTicketSale(id);
            if (ticketSale == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = _productService.AddTicketSaleToCart(ticketSale, userId);

            if (result)
            {
                return RedirectToAction("Index", "TicketSales");
            }

            return View(ticketSale);
        }

        private bool TicketSaleExists(Guid id)
        {
            return _productService.GetDetailsForTicketSale(id) != null;
        }
    }
}