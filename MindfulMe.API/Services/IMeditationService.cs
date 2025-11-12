using MindfulMe.API.DTOs;
using MindfulMe.API.Models;

namespace MindfulMe.API.Services
{
    public interface IMeditationService
    {
        Task<List<MeditationSession>> GetUserSessionsAsync(int userId);
        Task<MeditationSession?> GetSessionByIdAsync(int id);
        Task<MeditationSession> CreateSessionAsync(CreateMeditationSessionRequest request);
        Task<bool> UpdateSessionAsync(MeditationSession session);
        Task<bool> DeleteSessionAsync(int id, int userId);
    }
}