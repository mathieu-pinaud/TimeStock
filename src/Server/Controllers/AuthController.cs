using Microsoft.AspNetCore.Mvc;
using TimeStock.Shared.Dtos;
using TimeStock.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly DatabaseService _databaseService;
    private readonly JwtService _jwtService;
    private readonly ITenantCredentialsStore _credsStore;

    public AuthController(DatabaseService databaseService, JwtService jwtService, ITenantCredentialsStore credsStore)
    {
        _databaseService = databaseService;
        _jwtService = jwtService;
        _credsStore = credsStore;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDto userDto)
    {
        try
        {
            // Vérifie si un utilisateur avec cet email ou ce nom de compte existe déjà
            if (await _databaseService.UserExistsAsync(userDto.Email, userDto.AccountName))
                return BadRequest(new { message = "Email ou nom de compte déjà utilisé." });

            // Génération du nom de la base de données (basé sur le `AccountName`)
            string dbName = $"db_{userDto.AccountName}";

            // Génération d'un mot de passe unique pour la base de données
            string dbPassword = Guid.NewGuid().ToString("N").Substring(0, 16); // 16 caractères aléatoires

            // Création de l'utilisateur
            var newUser = new User
            {
                AccountName = userDto.AccountName,
                Name = userDto.Name,
                FirstName = userDto.FirstName,
                Email = userDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                DatabaseName = dbName,
                DatabasePassword = dbPassword
            };

            // Sauvegarde dans la DB principale + Création de la base de l'utilisateur
            await _databaseService.SaveUserAsync(newUser);

            var token = _jwtService.GenerateToken(newUser);
            var expiration = DateTime.UtcNow.AddMinutes(60);

            var response = new LoginResponseDto
            {
                Token = token,
                Expiration = expiration,
                Email = newUser.Email,
                AccountName = newUser.AccountName,
                DatabaseName = newUser.DatabaseName
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur d'inscription : {ex.Message}");
            return StatusCode(500, "Erreur lors de l'inscription.");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var user = await _databaseService.GetUserByEmailAsync(loginDto.Email);
            if (user == null)
                return Unauthorized(new { message = "Utilisateur introuvable" });

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Mot de passe incorrect" });

            var token = _jwtService.GenerateToken(user);
            var expiration = DateTime.UtcNow.AddMinutes(60); // à synchroniser plus tard si tu veux le rendre dynamique

            var response = new LoginResponseDto
            {
                Token = token,
                Expiration = expiration,
                Email = user.Email,
                AccountName = user.AccountName,
                DatabaseName = user.DatabaseName
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur de connexion : {ex.Message}");
            return StatusCode(500, "Erreur lors de la connexion.");
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MeResponseDto>> Me()
    {
        var user = HttpContext.User;
        var accountName = user.FindFirst("account")?.Value;
        if (string.IsNullOrEmpty(accountName))
        {
            return Unauthorized(new { message = "Account claim not found." });
        } try
        {
            await _credsStore.GetAsync(accountName);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }

        return new MeResponseDto
        {
            Email = user.FindFirst(ClaimTypes.Email)?.Value,
            AccountName = accountName,
            DatabaseName = user.FindFirst("db")?.Value
        };
    }
}
