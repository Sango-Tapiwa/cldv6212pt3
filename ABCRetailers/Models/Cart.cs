using System.ComponentModel.DataAnnotations;

namespace ABCRetailers.Models;

public class Cart
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string CustomerUsername { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ProductId { get; set; } = string.Empty;

    [Required]
    public int Quantity { get; set; }
}

public class CartSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public List<CartItemSession> Items { get; set; } = new();

    public decimal GetTotal()
    {
        return Items.Sum(i => i.Quantity * i.UnitPrice);
    }
}

public class CartItemSession
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? ImageUrl { get; set; }
}
