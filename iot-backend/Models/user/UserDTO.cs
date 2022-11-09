using iot_backend.Utils;

namespace iot_backend.Models.user;

public class UserDTO
{

    public UserDTO(User u)
    {
        UserId = u.UserId;
        Email = u.Email;
        Role = u.Role;
        PhoneExtension = u.PhoneExtension;
        PhoneNumber = u.PhoneNumber;
        Address = u.Address;
        JobDescription = u.JobDescription;
        isSetup = u.isSetup;
        Sensors = u.Sensors.Select(
            sensor => sensor.SensorId).ToList();
    }
    
    public int UserId { get; set; }
    public string Email { get; set; }
    
    public RoleEnum Role { get; set; }
    public string? PhoneExtension { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? JobDescription { get; set; }
    public bool isSetup { get; set; }
    
    public ICollection<String> Sensors { get; set; }
}