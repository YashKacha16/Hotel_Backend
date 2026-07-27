namespace Hotel_Backend.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string Joined { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhotoPath { get; set; }
    }
}
