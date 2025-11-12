using Microsoft.EntityFrameworkCore;
using MindfulMe.API.Data;
using MindfulMe.API.DTOs;
using MindfulMe.API.Models;

namespace MindfulMe.API.Services
{
    public class MeditationService : IMeditationService
    {
        private readonly AppDbContext _context;

        public MeditationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MeditationSession>> GetUserSessionsAsync(int userId)
        {
            return await _context.MeditationSessions
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.SessionDate)
                .ToListAsync();
        }

        public async Task<MeditationSession?> GetSessionByIdAsync(int id)
        {
            return await _context.MeditationSessions.FindAsync(id);
        }

        public async Task<MeditationSession> CreateSessionAsync(CreateMeditationSessionRequest request)
        {
            var session = new MeditationSession
            {
                UserId = request.UserId,
                SessionDate = DateTime.Now,
                DurationMinutes = request.DurationMinutes,
                MeditationType = request.MeditationType ?? "Breathing",
                QualityRating = request.QualityRating
            };

            _context.MeditationSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<bool> UpdateSessionAsync(MeditationSession session)
        {
            var existing = await _context.MeditationSessions.FindAsync(session.Id);
            if (existing == null || existing.UserId != session.UserId)
                return false;

            existing.SessionDate = session.SessionDate;
            existing.DurationMinutes = session.DurationMinutes;
            existing.MeditationType = session.MeditationType;
            existing.QualityRating = session.QualityRating;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSessionAsync(int id, int userId)
        {
            var session = await _context.MeditationSessions.FindAsync(id);
            if (session == null || session.UserId != userId)
                return false;

            _context.MeditationSessions.Remove(session);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}