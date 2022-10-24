using Microsoft.Build.Framework;

namespace iot_backend.Models.sensor;

public class SensorDataModel
{
    [Required]
    public double data { get; set; }
    
    [Required]
    public string SensorId { get; set; }
}