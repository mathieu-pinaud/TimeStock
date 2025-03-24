using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TimeStock.Shared.Dtos;
using TimeStock.Shared.Models;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public AuthController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
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
                PasswordHash = userDto.Password, // Déjà hashé côté client
                DatabaseName = dbName
            };

            // Sauvegarde dans la DB principale + Création de la base de l'utilisateur
            await _databaseService.SaveUserAsync(newUser, dbPassword);

            return Ok(new { message = "Utilisateur enregistré avec succès", database = dbName, dbPassword });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur d'inscription : {ex.Message}");
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

            // Vérifie le mot de passe
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Mot de passe incorrect" });

            return Ok(new
            {
                message = "Connexion réussie",
                database = user.DatabaseName
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur de connexion : {ex.Message}");
            return StatusCode(500, "Erreur lors de la connexion.");
        }
    }
}
