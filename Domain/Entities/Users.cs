using Domain.Enums;

namespace Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public UserStatus Status { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? EmailConfirmedAt { get; set; }
        public string? VerficationToken { get; set; }
        public DateTime? VerficationTokenExpiredAt { get; set; }

    }
}
