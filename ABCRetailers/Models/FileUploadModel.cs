using System.ComponentModel.DataAnnotations;

namespace ABCRetailers.Models;

public class FileUploadModel
{
    [Required(ErrorMessage = "Please select a file")]
    public IFormFile? ProofOfPayment { get; set; }

    [StringLength(100)]
    public string? OrderId { get; set; }

    [StringLength(200)]
    public string? CustomerName { get; set; }
}
