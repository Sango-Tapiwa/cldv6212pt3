using System.ComponentModel.DataAnnotations;

namespace ABCRetailers.Models;

public class Order
{
    public string Id { get; set; } = string.Empty;

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    [Required]
    public string ProductId { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public DateTimeOffset OrderDateUtc { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Submitted;

    public decimal TotalAmount => Quantity * UnitPrice;
}

public enum OrderStatus
{
    Submitted,
    Processing,
    Processed,
    Cancelled
}
