using System.ComponentModel.DataAnnotations;
using iot_backend.Utils;

namespace iot_backend.Models.login;

public class RegisterModel
{
    [Required]
    [EmailAddress]
    public string email { get; set; }
    
    [Required]
    public string password { get; set; }
    
    [Required]
    public RoleEnum role { get; set; }
}