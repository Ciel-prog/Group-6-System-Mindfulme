using Microsoft.EntityFrameworkCore;
using MindfulMe.Models;

namespace MindfulMe.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<MeditationSession> MeditationSessions { get; set; }
        

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=mindfulme.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed initial data
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "demo", Email = "demo@mindfulme.com", PasswordHash = User.HashPassword("demo") }
            );
        }
    }

    public class DatabaseService
    {
        private readonly AppDbContext _context;

        public DatabaseService()
        {
            _context = new AppDbContext();
            // Use migrations to update schema at startup
            _context.Database.Migrate();
        }

        // User operations
        public User? AuthenticateUser(string username, string password)
        {
            var hashedPassword = User.HashPassword(password);
            return _context.Users
                .FirstOrDefault(u => u.Username == username && u.PasswordHash == hashedPassword);
        }

        public User? GetUserById(int id)
        {
            return _context.Users.Find(id);
        }

        /// <summary>
        /// Register a new user. Returns Success flag, possible Error message and created User.
        /// Basic checks: unique username and unique email, minimal password length.
        /// </summary>
        public (bool Success, string? Error, User? User) RegisterUser(string username, string email, string password)
        {
            username = username?.Trim() ?? string.Empty;
            email = email?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Username, email and password are required.", null);
            }

            if (password.Length < 4)
            {
                return (false, "Password must be at least 4 characters long.", null);
            }

            if (_context.Users.Any(u => u.Username == username))
            {
                return (false, "Username already exists.", null);
            }

            if (_context.Users.Any(u => u.Email == email))
            {
                return (false, "Email already registered.", null);
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = User.HashPassword(password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return (true, null, user);
        }

        // Journal operations
        public void AddJournalEntry(JournalEntry entry)
        {
            _context.JournalEntries.Add(entry);
            _context.SaveChanges();
        }

        public List<JournalEntry> GetUserJournalEntries(int userId)
        {
            return _context.JournalEntries
                .Where(j => j.UserId == userId)
                .OrderByDescending(j => j.EntryDate)
                .ToList();
        }

        /// <summary>
        /// Retrieve a single journal entry by id.
        /// </summary>
        public JournalEntry? GetJournalEntryById(int id)
        {
            return _context.JournalEntries.Find(id);
        }

        /// <summary>
        /// Update a journal entry. Returns Success flag and optional error message.
        /// Ownership check: will ensure the updated entry's UserId matches the stored entry.
        /// </summary>
        public (bool Success, string? Error) UpdateJournalEntry(JournalEntry updatedEntry)
        {
            var existing = _context.JournalEntries.Find(updatedEntry.Id);
            if (existing == null)
                return (false, "Journal entry not found.");

            if (existing.UserId != updatedEntry.UserId)
                return (false, "Unauthorized: user does not own this entry.");

            // Update allowed fields
            existing.EntryDate = updatedEntry.EntryDate;
            existing.Content = updatedEntry.Content;
            existing.MoodScore = updatedEntry.MoodScore;
            existing.StressLevel = updatedEntry.StressLevel;
            existing.HoursSlept = updatedEntry.HoursSlept;

            try
            {
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Delete a journal entry by id. Returns true if deleted, false otherwise.
        /// Ownership check: requires userId to match the entry's owner.
        /// </summary>
        public (bool Success, string? Error) DeleteJournalEntry(int id, int userId)
        {
            var existing = _context.JournalEntries.Find(id);
            if (existing == null)
                return (false, "Journal entry not found.");

            if (existing.UserId != userId)
                return (false, "Unauthorized: cannot delete another user's entry.");

            _context.JournalEntries.Remove(existing);
            try
            {
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Meditation operations
        public void AddMeditationSession(MeditationSession session)
        {
            _context.MeditationSessions.Add(session);
            _context.SaveChanges();
        }

        public List<MeditationSession> GetUserMeditationSessions(int userId)
        {
            return _context.MeditationSessions
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.SessionDate)
                .ToList();
        }

        /// <summary>
        /// Retrieve a single meditation session by id.
        /// </summary>
        public MeditationSession? GetMeditationSessionById(int id)
        {
            return _context.MeditationSessions.Find(id);
        }

        /// <summary>
        /// Update a meditation session. Returns Success flag and optional error message.
        /// Ownership check ensures the session belongs to the provided user.
        /// </summary>
        public (bool Success, string? Error) UpdateMeditationSession(MeditationSession updatedSession)
        {
            var existing = _context.MeditationSessions.Find(updatedSession.Id);
            if (existing == null)
                return (false, "Meditation session not found.");

            if (existing.UserId != updatedSession.UserId)
                return (false, "Unauthorized: user does not own this session.");

            // Update allowed fields
            existing.SessionDate = updatedSession.SessionDate;
            existing.DurationMinutes = updatedSession.DurationMinutes;
            existing.MeditationType = updatedSession.MeditationType;
            existing.QualityRating = updatedSession.QualityRating;

            try
            {
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Delete a meditation session by id. Returns success flag and optional error.
        /// </summary>
        public (bool Success, string? Error) DeleteMeditationSession(int id, int userId)
        {
            var existing = _context.MeditationSessions.Find(id);
            if (existing == null)
                return (false, "Meditation session not found.");

            if (existing.UserId != userId)
                return (false, "Unauthorized: cannot delete another user's session.");

            _context.MeditationSessions.Remove(existing);
            try
            {
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Goals operations (new)
     
       
      
      
     

        // Analytics data
        public List<JournalEntry> GetEntriesFromLastDays(int userId, int days)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            return _context.JournalEntries
                .Where(j => j.UserId == userId && j.EntryDate >= cutoffDate)
                .ToList();
        }

        // Helper: distinct activity dates (date component only), descending
        public List<DateTime> GetUserJournalEntryDates(int userId)
        {
            return _context.JournalEntries
                .Where(j => j.UserId == userId)
                .Select(j => j.EntryDate.Date) // normalize to date
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();
        }

        public List<DateTime> GetUserMeditationSessionDates(int userId)
        {
            return _context.MeditationSessions
                .Where(m => m.UserId == userId)
                .Select(m => m.SessionDate.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();
        }
    }
}