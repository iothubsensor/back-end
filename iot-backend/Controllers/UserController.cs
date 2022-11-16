using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using iot_backend.Models;
using iot_backend.Models.login;
using iot_backend.Models.user;
using iot_backend.Repository;
using iot_backend.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;

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
        
        if (!_context.Users.Any())
        {
            _context.Users.Add(new User("admin@sensorify.com", generatePassword("admin123"), RoleEnum.ADMIN));
            _context.SaveChanges();
        }
    }

    [HttpPost("register")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<UserDTO>> register([FromBody]RegisterModel user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "The model is invalid.", code = BadRequest().StatusCode});
        }
        
        var dbUser = _context.Users.Add(new User(user.email, generatePassword(user.password), user.role));
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
            var searchedUser = await _context.Users.Where(u => u.Email == user.email).Include(u => u.Sensors).FirstAsync();
            
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
    public async Task<ActionResult<UserDTO>> get(int id)
    {
        if (_context.Users == null) return NotFound();
        var user = await _context.Users.Include(u => u.Sensors).FirstAsync(u => u.UserId == id);

        if (user == null) return NotFound();

        return new UserDTO(user);
    }
    
    [HttpPost("setup")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<UserDTO>> setup([FromBody]SetupModel setupModel) 
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "The model is invalid.", code = BadRequest().StatusCode});
        }
        
        var userId = Int32.Parse(User.Claims.First().Value);
        
        try
        {
            var searchedUser = await _context.Users.Where(u => u.UserId == userId).FirstAsync();

            searchedUser.isSetup = true;
            
            searchedUser.PhoneExtension = setupModel.phone_ext;
            searchedUser.PhoneNumber = setupModel.phone_number;
            searchedUser.JobDescription = setupModel.job_description;
            searchedUser.Address = setupModel.address;

            _context.Users.Update(searchedUser);
            await _context.SaveChangesAsync();
            
            return Ok(new UserDTO(searchedUser));
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "The email provided is incorrect", code = NotFound().StatusCode });
        }
    }
    
    [HttpPost("sensor/toggle")]
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

        bool doesRelationshipExist = await _context.UserSensors.AnyAsync(uS => uS.SensorId == sensorId && uS.UserId.ToString() == id);
        
        if(doesRelationshipExist)
            _context.UserSensors.Remove(new UserSensor(Int32.Parse(id), sensorId));
        else
            _context.UserSensors.Add(new UserSensor(Int32.Parse(id), sensorId));
        
        await _context.SaveChangesAsync();
        
        return Ok(new
            { message = "Successfully " + (doesRelationshipExist ? "un" : "") + "toggled sensor", code = Ok().StatusCode});
    }

    
    private bool UserExists(int id)
    {
        return (_context.Users?.Any(e => e.UserId == id)).GetValueOrDefault();
    }
    
    private String generatePassword(String unhashed)
    {
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
        var pbkdf2 = new Rfc2898DeriveBytes(unhashed, salt, 10000);
        byte[] hash = pbkdf2.GetBytes(20);
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);
        
        return Convert.ToBase64String(hashBytes);
    }
}