using System.Text.Json.Serialization;

namespace iot_backend.Models;

public class UserSensor
{
    public UserSensor()
    {
        
    }
    
    public UserSensor(int userId, string sensorId)
    {
        UserId = userId;
        SensorId = sensorId;
    }

    public int UserId { get; set; }
    
    [JsonIgnore]
    public virtual User User { get; set; }

    public string SensorId { get; set; }
    
    [JsonIgnore]
    public virtual Sensor Sensor { get; set; }
    
    protected bool Equals(UserSensor other)
    {
        return UserId == other.UserId && SensorId == other.SensorId;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((UserSensor)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UserId, SensorId);
    }
}