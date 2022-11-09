using System.ComponentModel.DataAnnotations;
using iot_backend.Utils;
using Newtonsoft.Json;

namespace iot_backend.Models;

public partial class User
{
    public User()
    {
        Sensors = new HashSet<UserSensor>();
    }
    
    public User(string email, string password, RoleEnum role) : this()
    {
        Email = email;
        Password = password;
        Role = role;
        isSetup = false;
    }

    public int UserId { get; set; }
    
    public string Email { get; set; }
    
    public string Password { get; set; }
    
    public RoleEnum Role { get; set; }
    
    public string? PhoneExtension { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? JobDescription { get; set; }
    
    public bool isSetup { get; set; }
    
    public virtual ICollection<UserSensor> Sensors { get; set; }
}