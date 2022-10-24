using System.Text.Json.Serialization;

namespace iot_backend.Models;

public partial class Sensor
{
    public string SensorId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public virtual ICollection<SensorData> SensorDatas { get; set; }
  
    public virtual ICollection<UserSensor> UserSensors { get; set; }
}