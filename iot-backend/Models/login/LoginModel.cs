using System.ComponentModel.DataAnnotations;

namespace iot_backend.Models.login;

public class LoginModel
{
    [EmailAddress]
    [Required]
    public string email { get; set; }
    
    [Required]
    public string password { get; set; }
}