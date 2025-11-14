using System.ComponentModel.DataAnnotations;

namespace ABCRetailers.Models;

public class Product
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Stock must be 0 or greater")]
    public int StockAvailable { get; set; }

    public string? ImageUrl { get; set; }
}
