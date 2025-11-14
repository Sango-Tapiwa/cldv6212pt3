using ABCRetailers.Models;
using ABCRetailers.Models.ViewModels;
using ABCRetailers.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ABCRetailers.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFunctionsApi _api;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IFunctionsApi api, ILogger<HomeController> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchTerm = "")
        {
            try
            {
                var productsTask = _api.GetProductsAsync();
                var customersTask = _api.GetCustomersAsync();
                var ordersTask = _api.GetOrdersAsync();

                await Task.WhenAll(productsTask, customersTask, ordersTask);

                var products = productsTask.Result ?? new List<Product>();
                var customers = customersTask.Result ?? new List<Customer>();
                var orders = ordersTask.Result ?? new List<Order>();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    products = products.Where(p =>
                        p.ProductName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (p.Description != null && p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                var vm = new HomeViewModel
                {
                    FeaturedProducts = products.Take(8).ToList(),
                    ProductCount = products.Count,
                    CustomerCount = customers.Count,
                    OrderCount = orders.Count,
                    SearchTerm = searchTerm
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dashboard data from Functions API.");
                TempData["Error"] = "Could not load dashboard data. Please try again.";
                return View(new HomeViewModel());
            }
        }

        public async Task<IActionResult> AdminDashboard()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return View("_AccessDenied");
            }

            try
            {
                var productsTask = _api.GetProductsAsync();
                var customersTask = _api.GetCustomersAsync();
                var ordersTask = _api.GetOrdersAsync();

                await Task.WhenAll(productsTask, customersTask, ordersTask);

                var vm = new HomeViewModel
                {
                    FeaturedProducts = productsTask.Result?.Take(8).ToList() ?? new List<Product>(),
                    ProductCount = productsTask.Result?.Count ?? 0,
                    CustomerCount = customersTask.Result?.Count ?? 0,
                    OrderCount = ordersTask.Result?.Count ?? 0
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load admin dashboard");
                TempData["Error"] = "Could not load dashboard data";
                return View(new HomeViewModel());
            }
        }

        public async Task<IActionResult> CustomerDashboard()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                var products = await _api.GetProductsAsync();
                var vm = new HomeViewModel
                {
                    FeaturedProducts = products.Take(8).ToList()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load customer dashboard");
                TempData["Error"] = "Could not load products";
                return View(new HomeViewModel());
            }
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
