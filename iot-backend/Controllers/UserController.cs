using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using iot_backend.Models;
using iot_backend.Models.login;
using iot_backend.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace iot_backend;

[Route("api/user")]
[Produces("application/json")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IOTContext _context;
    private readonly IJWTAuthManager _authentication;

    public UserController(IOTContext context, IJWTAuthManager authentication)
    {
        _context = context;
        _authentication = authentication;
    }
    
    [HttpPost("register")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<User>> register([FromBody]RegisterModel user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "The model is invalid.", code = BadRequest().StatusCode});
        }

        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
        var pbkdf2 = new Rfc2898DeriveBytes(user.password, salt, 10000);
        byte[] hash = pbkdf2.GetBytes(20);
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);
        string savedPasswordHash = Convert.ToBase64String(hashBytes);
        
        var dbUser = _context.Users.Add(new User(user.email, savedPasswordHash, user.role));
        await _context.SaveChangesAsync();
        
        return CreatedAtAction("get", new { id = dbUser.Entity.UserId }, dbUser.Entity);
    }
    
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> login([FromBody]LoginModel user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "The model is invalid.", code = BadRequest().StatusCode});
        }
        
        try
        {
            var searchedUser = await _context.Users.Where(u => u.Email == user.email).FirstAsync();
            
            string savedPasswordHash = searchedUser.Password;
            byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
            byte[] salt = new byte[16];
            
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(user.password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            
            for (int i=0; i < 20; i++)
                if (hashBytes[i+16] != hash[i])
                    return Unauthorized(new {message = "The given password is incorrect", code = Unauthorized().StatusCode});
            
            var token = _authentication.GenerateJWT(searchedUser);
            return Ok(token);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "The email provided is incorrect", code = NotFound().StatusCode});
        }
    }
    
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<User>> get(int id)
    {
        if (_context.Users == null) return NotFound();
        var user = await _context.Users.FindAsync(id);

        if (user == null) return NotFound();

        return user;
    }
    
    [HttpPost("setup")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> setup([FromBody]SetupModel setupModel) 
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "The model is invalid.", code = BadRequest().StatusCode});
        }
        
        var email = User.Claims.First().Value;

        try
        {
            var searchedUser = await _context.Users.Where(u => u.Email == email).FirstAsync();

            searchedUser.isSetup = true;
            
            searchedUser.PhoneExtension = setupModel.phone_ext;
            searchedUser.PhoneNumber = setupModel.phone_number;
            searchedUser.JobDescription = setupModel.job_description;
            searchedUser.Address = setupModel.address;

            _context.Users.Update(searchedUser);
            await _context.SaveChangesAsync();
            
            return Ok(searchedUser);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "The email provided is incorrect", code = NotFound().StatusCode });
        }
    }
    
    [HttpPost("sensor/attach")]
    [Authorize]
    public async Task<ActionResult<UserSensor>> attach([FromBody]string sensorId)
    {
        var id = User.Claims.First(claim => claim.Type == "user_id").Value;
        bool doesUserExist = await (_context.Users?.AnyAsync(e => e.UserId.ToString() == id));
        
        if (!doesUserExist)
        {
            return NotFound(new { message = "The user id authenticated is incorrect", code = NotFound().StatusCode });
        }
        
        bool doesSensorExist = await (_context.Sensors?.AnyAsync(e => e.SensorId == sensorId));

        if (!doesSensorExist)
        {
            return NotFound(new { message = "The sensor id is incorrect", code = NotFound().StatusCode });
        }

        var userSensor = _context.UserSensors.Add(new UserSensor(Int32.Parse(id), sensorId));
        await _context.SaveChangesAsync();
        
        return Ok(new
            { message = "Successfully attached sensor", code = Ok().StatusCode, data = userSensor.Entity });
    }

    
    private bool UserExists(int id)
    {
        return (_context.Users?.Any(e => e.UserId == id)).GetValueOrDefault();
    }
}