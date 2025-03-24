namespace TimeStock.Shared.Models
{
    public class User
    {
        public int Id { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty; // Stocke la DB de l'utilisateur
    }
}