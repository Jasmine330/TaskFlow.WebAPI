// TaskFlow.WebAPI/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskFlow.WebAPI.Data;
using TaskFlow.WebAPI.Models; // Assuming User model is here
using Microsoft.EntityFrameworkCore; // Needed for ToListAsync, FirstOrDefaultAsync

// Define simple models for request data (DTOs)
public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Password);
public record AuthResponse(string Token, string Username);


namespace TaskFlow.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // --- VERY IMPORTANT: HASH THE PASSWORD ---
            // In a real app, use a strong hashing library like BCrypt.Net or ASP.NET Core Identity's hasher.
            // Example using a simple (NOT SECURE FOR PRODUCTION) placeholder:
            string insecurePasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(request.Password)); // DO NOT USE THIS IN PRODUCTION

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (existingUser != null)
            {
                return BadRequest("Username already exists.");
            }

            var user = new User
            {
                Username = request.Username,
                PasswordHash = insecurePasswordHash // Store the hash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully" });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            // --- VERY IMPORTANT: COMPARE HASHES ---
            // You would hash the provided password and compare it to the stored hash.
            string insecurePasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(request.Password)); // DO NOT USE THIS IN PRODUCTION

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // Check if user exists and if the password hash matches
            if (user == null || user.PasswordHash != insecurePasswordHash)
            {
                return Unauthorized("Invalid username or password.");
            }

            // --- Generate JWT Token ---
            var token = GenerateJwtToken(user);

            return Ok(new AuthResponse(token, user.Username));
        }


        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)); // Use ! to assert Key is not null
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Claims identify the user and their permissions (we'll keep it simple)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username), // Subject (usually username or user ID)
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()), // User ID
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token identifier
                // You can add more claims like roles here: new Claim(ClaimTypes.Role, "Admin")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Token expiration time (e.g., 1 hour)
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token); // Serialize the token to a string
        }
    }
}