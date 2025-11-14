using System.ComponentModel.DataAnnotations;

namespace ABCRetailers.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
