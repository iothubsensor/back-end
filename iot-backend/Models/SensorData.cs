using System.Text.Json.Serialization;

namespace iot_backend.Models;

public partial class SensorData
{
    public int SensorDataId { get; set; }
    
    public double Data { get; set; }
    public DateTime Date { get; set; }
    
    public string SensorId { get; set; }
    
    public virtual Sensor Sensor { get; set; }
}