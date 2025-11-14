using System.ComponentModel.DataAnnotations;

namespace ABCRetailers.Models.ViewModels;

public class OrderCreateViewModel
{
    [Required(ErrorMessage = "Please select a customer")]
    public string CustomerId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a product")]
    public string ProductId { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;

    public List<Customer> Customers { get; set; } = new();
    public List<Product> Products { get; set; } = new();
}
