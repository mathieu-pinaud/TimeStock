using System.ComponentModel.DataAnnotations;

namespace TimeStock.Shared.Dtos
{
    public class UserDto
    {
        [Required(ErrorMessage = "Le nom de compte est requis.")]
        [MinLength(3, ErrorMessage = "Le nom de compte doit contenir au moins 3 caractères.")]
        public string AccountName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est requis.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est requis.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Format d'email invalide.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
        public string Password { get; set; } = string.Empty;
    }
}
