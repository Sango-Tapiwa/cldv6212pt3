using ABCRetailers.Models;
using ABCRetailers.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ABCRetailers.Controllers;

public class CartController : Controller
{
    private readonly IFunctionsApi _api;
    private const string CartSessionKey = "ShoppingCart";

    public CartController(IFunctionsApi api)
    {
        _api = api;
    }

    private CartSession GetCart()
    {
        var cartJson = HttpContext.Session.GetString(CartSessionKey);
        if (string.IsNullOrEmpty(cartJson))
        {
            var cart = new CartSession
            {
                Username = HttpContext.Session.GetString("Username") ?? "Guest"
            };
            return cart;
        }
        return JsonSerializer.Deserialize<CartSession>(cartJson) ?? new CartSession();
    }

    private void SaveCart(CartSession cart)
    {
        var cartJson = JsonSerializer.Serialize(cart);
        HttpContext.Session.SetString(CartSessionKey, cartJson);
    }

    public IActionResult Index()
    {
        var cart = GetCart();
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(string productId, int quantity = 1)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
        {
            TempData["Error"] = "Please log in to add items to cart";
            return RedirectToAction("Index", "Login");
        }

        var product = await _api.GetProductAsync(productId);
        if (product == null)
        {
            TempData["Error"] = "Product not found";
            return RedirectToAction("Index", "Home");
        }

        if (product.StockAvailable < quantity)
        {
            TempData["Error"] = $"Only {product.StockAvailable} items available";
            return RedirectToAction("Index", "Home");
        }

        var cart = GetCart();
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Items.Add(new CartItemSession
            {
                ProductId = productId,
                ProductName = product.ProductName,
                Quantity = quantity,
                UnitPrice = product.Price,
                ImageUrl = product.ImageUrl
            });
        }

        SaveCart(cart);
        TempData["Success"] = $"{product.ProductName} added to cart";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult UpdateQuantity(string productId, int quantity)
    {
        var cart = GetCart();
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item != null)
        {
            if (quantity <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            SaveCart(cart);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult RemoveFromCart(string productId)
    {
        var cart = GetCart();
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item != null)
        {
            cart.Items.Remove(item);
            SaveCart(cart);
            TempData["Success"] = "Item removed from cart";
        }

        return RedirectToAction("Index");
    }

    public IActionResult ClearCart()
    {
        HttpContext.Session.Remove(CartSessionKey);
        TempData["Success"] = "Cart cleared";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Checkout()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            TempData["Error"] = "Please log in to checkout";
            return RedirectToAction("Index", "Login");
        }

        var cart = GetCart();
        if (!cart.Items.Any())
        {
            TempData["Error"] = "Your cart is empty";
            return RedirectToAction("Index");
        }

        var customers = await _api.GetCustomersAsync();
        var customer = customers.FirstOrDefault(c => c.Username == username);

        if (customer == null)
        {
            TempData["Error"] = "Customer profile not found";
            return RedirectToAction("Index", "Login");
        }

        var createdOrders = new List<Order>();

        foreach (var item in cart.Items)
        {
            try
            {
                var order = await _api.CreateOrderAsync(customer.Id, item.ProductId, item.Quantity);
                createdOrders.Add(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating order for {item.ProductName}: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        HttpContext.Session.Remove(CartSessionKey);
        TempData["Success"] = $"{createdOrders.Count} order(s) placed successfully!";
        return RedirectToAction("Confirmation", new { orderIds = string.Join(",", createdOrders.Select(o => o.Id)) });
    }

    public async Task<IActionResult> Confirmation(string orderIds)
    {
        if (string.IsNullOrEmpty(orderIds))
        {
            return RedirectToAction("Index", "Home");
        }

        var ids = orderIds.Split(',');
        var orders = new List<Order>();

        foreach (var id in ids)
        {
            var order = await _api.GetOrderAsync(id.Trim());
            if (order != null)
            {
                orders.Add(order);
            }
        }

        return View(orders);
    }
}
