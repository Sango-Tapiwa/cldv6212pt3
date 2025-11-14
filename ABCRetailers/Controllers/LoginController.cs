using ABCRetailers.Data;
using ABCRetailers.Models;
using ABCRetailers.Models.ViewModels;
using ABCRetailers.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABCRetailers.Controllers;

public class LoginController : Controller
{
    private readonly AuthDbContext _db;
    private readonly IFunctionsApi _api;

    public LoginController(AuthDbContext db, IFunctionsApi api)
    {
        _db = db;
        _api = api;
    }

    public IActionResult Index() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == model.Username && u.PasswordHash == model.Password);

        if (user == null)
        {
            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }

        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetString("Role", user.Role);

        TempData["Success"] = $"Welcome back, {user.Username}!";

        if (user.Role == "Admin")
            return RedirectToAction("AdminDashboard", "Home");
        else
            return RedirectToAction("CustomerDashboard", "Home");
    }

    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
        if (existingUser != null)
        {
            ModelState.AddModelError("Username", "Username already exists");
            return View(model);
        }

        var user = new User
        {
            Username = model.Username,
            PasswordHash = model.Password,
            Role = "Customer"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var customer = new Customer
        {
            Name = model.Name,
            Surname = model.Surname,
            Username = model.Username,
            Email = model.Email,
            ShippingAddress = model.ShippingAddress
        };

        try
        {
            await _api.CreateCustomerAsync(customer);
        }
        catch (Exception ex)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            ModelState.AddModelError("", $"Error creating customer profile: {ex.Message}");
            return View(model);
        }

        TempData["Success"] = "Registration successful! Please log in.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "You have been logged out.";
        return RedirectToAction("Index", "Home");
    }
}
