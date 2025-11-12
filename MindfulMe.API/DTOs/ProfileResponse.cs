namespace MindfulMe.API.DTOs
{
    public class ProfileResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateProfileRequest
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class DeleteAccountRequest
    {
        public int UserId { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class UserStatsResponse
    {
        public int TotalJournalEntries { get; set; }
        public int TotalMeditationSessions { get; set; }
        public int TotalMeditationMinutes { get; set; }
        public DateTime MemberSince { get; set; }
        public int CurrentStreak { get; set; }
    }
}