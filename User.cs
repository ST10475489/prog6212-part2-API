namespace RaceDay.Api.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public static class Roles
    {
        public const string Organiser = "Organiser";
        public const string Participant = "Participant";

        // Returns the properly cased role, or null if the role is not valid
        public static string Normalise(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return null;
            if (role.Trim().ToLower() == "organiser") return Organiser;
            if (role.Trim().ToLower() == "participant") return Participant;
            return null;
        }
    }
}
