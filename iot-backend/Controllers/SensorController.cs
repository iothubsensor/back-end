using System.Security.Cryptography;
using iot_backend.Models;
using iot_backend.Models.login;
using iot_backend.Models.sensor;
using iot_backend.Models.user;
using iot_backend.Repository;
using iot_backend.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace iot_backend;

[Route("api/sensor")]
[Produces("application/json")]
[ApiController]
public class SensorController: ControllerBase
{
    private readonly IOTContext _context;
    private readonly IJWTAuthManager _authentication;
    private static Random random = new Random();
    
    public SensorController(IOTContext context, IJWTAuthManager authentication)
    {
        _context = context;
        _authentication = authentication;
    }
    
    [HttpPost("register")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<SensorDTO>> register([FromBody]SensorModel sensorModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "The model is invalid.", code = BadRequest().StatusCode});
        }

        Sensor sensor = new Sensor();
        
        sensor.SensorId = getRandomSensorKey();
        sensor.Name = sensorModel.name;
        sensor.Description = sensorModel.description;
        sensor.Key = getRandomSensorKey();
        
        var dbSensor = _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();
        
        return Ok(new
            { message = "Successfully created sensor", code = Ok().StatusCode, token = sensor.Key, data = new SensorDTO(dbSensor.Entity) });
    }

    [HttpGet("get/{id}")]
    [Authorize]
    public async Task<ActionResult<SensorDTO>> get(string id)
    {
        var dbSensor = await _context.Sensors.Include(sensor => sensor.Datas).FirstOrDefaultAsync(s => s.SensorId == id);
            
        if(dbSensor == null)
            return NotFound(new { message = "The sensor with that id has not been found.", code = NotFound().StatusCode});
        
        
        return Ok(new
            { message = "Successfully retrieved sensor data " + dbSensor.Datas.Count, code = Ok().StatusCode, data = new SensorDTO(dbSensor) });
    }
    
    [HttpPost("data")]
    public async Task<ActionResult<SensorDataDTO>> postData([FromBody]SensorDataModel sensorDataModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "The model is invalid.", code = BadRequest().StatusCode});
        }
        
        var sensorExists = await (_context.Sensors?.AnyAsync(e => e.SensorId == sensorDataModel.SensorId && e.Key == sensorDataModel.key));
        
        if(!sensorExists)
            return NotFound(new { message = "The sensor with that id has not been found.", code = NotFound().StatusCode});
        
        SensorData sensorData = new SensorData();
        
        sensorData.Data = sensorDataModel.data;
        sensorData.Date = DateTime.Now;
        sensorData.SensorId = sensorDataModel.SensorId;
        
        var dbSensorData = _context.SensorDatas.Add(sensorData);
        await _context.SaveChangesAsync();
        
        return Ok(new
            { message = "Successfully added data", code = Ok().StatusCode, data = new SensorDataDTO(dbSensorData.Entity) });
    }
    
    [HttpGet("get")]
    [Authorize]
    public async Task<ActionResult<SensorDTO>> getSensors()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "The model is invalid.", code = BadRequest().StatusCode});
        }

        var dbSensors = await _context.Sensors.Include(s => s.Users).Select(sensor => new SensorDTO(sensor)).ToListAsync();

        return Ok(new
            { message = "Successfully found sensors", code = Ok().StatusCode, data = dbSensors });
    }

    public static string getRandomSensorKey()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
        
        return new string(Enumerable.Repeat(chars, 16)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}