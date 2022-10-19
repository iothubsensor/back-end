namespace iot_backend.Models;

public partial class Sensor
{
    public int SensorId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public virtual ICollection<SensorData> SensorDatas { get; set; }
    public virtual ICollection<UserSensor> UserSensors { get; set; }
}