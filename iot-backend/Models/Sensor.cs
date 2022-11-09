using System.Text.Json.Serialization;
using iot_backend.Models.user;

namespace iot_backend.Models;

public partial class Sensor
{
    public Sensor()
    {
        Datas = new List<SensorData>();
        Users = new List<UserSensor>();
    }
    
    public string SensorId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public string Key { get; set; }

    public virtual ICollection<SensorData> Datas { get; set; }
    public virtual ICollection<UserSensor> Users { get; set; }
}