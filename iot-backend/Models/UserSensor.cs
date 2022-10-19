namespace iot_backend.Models;

public class UserSensor
{
    public int UserId { get; set; }
    public virtual User User { get; set; }

    public int SensorId { get; set; }
    public virtual Sensor Sensor { get; set; }
}