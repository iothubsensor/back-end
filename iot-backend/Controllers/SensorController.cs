using System.Security.Cryptography;
using iot_backend.Models;
using iot_backend.Models.login;
using iot_backend.Models.sensor;
using iot_backend.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<Sensor>> register([FromBody]SensorModel sensorModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "The model is invalid.", code = BadRequest().StatusCode});
        }

        Sensor sensor = new Sensor();
        
        sensor.SensorId = getRandomSensorKey();
        sensor.Name = sensorModel.name;
        sensor.Description = sensorModel.description;
        
        var dbSensor = _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();
        
        return Ok(new
            { message = "Successfully created sensor", code = Ok().StatusCode, data = dbSensor.Entity });
    }

    [HttpGet("get/{id}")]
    [Authorize]
    public async Task<ActionResult<Sensor>> get(string id)
    {
        var dbSensor = await _context.Sensors.Include(sensor => sensor.SensorDatas).FirstOrDefaultAsync(s => s.SensorId == id);
            
        if(dbSensor == null)
            return NotFound(new { message = "The sensor with that id has not been found.", code = NotFound().StatusCode});
        
        
        return Ok(new
            { message = "Successfully retrieved sensor data " + dbSensor.SensorDatas.Count, code = Ok().StatusCode, data = dbSensor });
    }
    
    [HttpPost("data")]
    public async Task<ActionResult<Sensor>> postData([FromBody]SensorDataModel sensorDataModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "The model is invalid.", code = BadRequest().StatusCode});
        }
        
        var sensorExists = await (_context.Sensors?.AnyAsync(e => e.SensorId == sensorDataModel.SensorId));
        
        if(!sensorExists)
            return NotFound(new { message = "The sensor with that id has not been found.", code = NotFound().StatusCode});
        
        SensorData sensorData = new SensorData();
        
        sensorData.Data = sensorDataModel.data;
        sensorData.Date = DateTime.Now;
        sensorData.SensorId = sensorDataModel.SensorId;
        
        var dbSensorData = _context.SensorDatas.Add(sensorData);
        await _context.SaveChangesAsync();
        
        return Ok(new
            { message = "Successfully added data", code = Ok().StatusCode, data = dbSensorData.Entity });
    }
    
    [HttpGet("get")]
    [Authorize]
    public async Task<ActionResult<Sensor>> getSensors()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "The model is invalid.", code = BadRequest().StatusCode});
        }

        var dbSensors = await _context.Sensors.ToListAsync();

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