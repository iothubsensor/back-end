namespace iot_backend.Models.user;

public class SensorDTO
{
    public SensorDTO(Sensor sensor)
    {
        SensorId = sensor.SensorId;
        Name = sensor.Name;
        Description = sensor.Description;
        
        Datas = sensor.Datas.Select(
            data => new SensorDataDTO(data)).ToList();

        Users = sensor.Users.Select(
            user => user.UserId).ToList();
    }
    
    public string SensorId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public ICollection<SensorDataDTO> Datas { get; set; }
    public ICollection<Int32> Users { get; set; }
}