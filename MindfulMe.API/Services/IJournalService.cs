using MindfulMe.API.DTOs;
using MindfulMe.API.Models;

namespace MindfulMe.API.Services
{
    public interface IJournalService
    {
        Task<List<JournalEntry>> GetUserEntriesAsync(int userId);
        Task<JournalEntry?> GetEntryByIdAsync(int id);
        Task<JournalEntry> CreateEntryAsync(CreateJournalEntryRequest request);
        Task<bool> UpdateEntryAsync(JournalEntry entry);
        Task<bool> DeleteEntryAsync(int id, int userId);
    }
}