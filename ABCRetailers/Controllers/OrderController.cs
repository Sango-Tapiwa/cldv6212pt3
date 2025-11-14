using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Models;
using ABCRetailers.Models.ViewModels;
using ABCRetailers.Services;

namespace ABCRetailers.Controllers
{
    public class OrderController : Controller
    {
        private readonly IFunctionsApi _api;
        public OrderController(IFunctionsApi api) => _api = api;

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return View("_AccessDenied");
            }

            var orders = await _api.GetOrdersAsync();
            return View(orders.OrderByDescending(o => o.OrderDateUtc).ToList());
        }

        public async Task<IActionResult> MyOrders()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Login");
            }

            var orders = await _api.GetOrdersAsync();
            var myOrders = orders.Where(o => o.Username == username)
                                 .OrderByDescending(o => o.OrderDateUtc)
                                 .ToList();
            return View(myOrders);
        }

        public async Task<IActionResult> Manage()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return View("_AccessDenied");
            }

            var orders = await _api.GetOrdersAsync();
            return View(orders.OrderByDescending(o => o.OrderDateUtc).ToList());
        }

        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return View("_AccessDenied");
            }

            var customers = await _api.GetCustomersAsync();
            var products = await _api.GetProductsAsync();

            var vm = new OrderCreateViewModel
            {
                Customers = customers,
                Products = products
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                var customer = await _api.GetCustomerAsync(model.CustomerId);
                var product = await _api.GetProductAsync(model.ProductId);

                if (customer is null || product is null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid customer or product selected.");
                    await PopulateDropdowns(model);
                    return View(model);
                }

                if (product.StockAvailable < model.Quantity)
                {
                    ModelState.AddModelError("Quantity", $"Insufficient stock. Available: {product.StockAvailable}");
                    await PopulateDropdowns(model);
                    return View(model);
                }

                var saved = await _api.CreateOrderAsync(model.CustomerId, model.ProductId, model.Quantity);

                TempData["Success"] = "Order created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating order: {ex.Message}");
                await PopulateDropdowns(model);
                return View(model);
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            var order = await _api.GetOrderAsync(id);
            return order is null ? NotFound() : View(order);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return View("_AccessDenied");
            }

            if (string.IsNullOrWhiteSpace(id)) return NotFound();
            var order = await _api.GetOrderAsync(id);
            return order is null ? NotFound() : View(order);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Order posted)
        {
            if (!ModelState.IsValid) return View(posted);

            try
            {
                await _api.UpdateOrderStatusAsync(posted.Id, posted.Status.ToString());
                TempData["Success"] = "Order status updated successfully!";
                return RedirectToAction(nameof(Manage));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating order: {ex.Message}");
                return View(posted);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["Error"] = "Access denied";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                await _api.DeleteOrderAsync(id);
                TempData["Success"] = "Order deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting order: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<JsonResult> GetProductPrice(string productId)
        {
            try
            {
                var product = await _api.GetProductAsync(productId);
                if (product is not null)
                {
                    return Json(new
                    {
                        success = true,
                        price = product.Price,
                        stock = product.StockAvailable,
                        productName = product.ProductName
                    });
                }
                return Json(new { success = false });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(string id, string newStatus)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Access denied" });
            }

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(newStatus))
            {
                return Json(new { success = false, message = "Invalid order ID or status" });
            }

            try
            {
                await _api.UpdateOrderStatusAsync(id, newStatus);
                return Json(new { success = true, message = $"Order status updated to {newStatus}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task PopulateDropdowns(OrderCreateViewModel model)
        {
            model.Customers = await _api.GetCustomersAsync();
            model.Products = await _api.GetProductsAsync();
        }
    }
}
