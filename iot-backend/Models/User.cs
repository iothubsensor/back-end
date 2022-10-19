namespace iot_backend.Models;

public partial class User
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    
    public string PhoneExtension { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string JobDescription { get; set; }
    
    public virtual ICollection<UserSensor> UserSensors { get; set; }
}