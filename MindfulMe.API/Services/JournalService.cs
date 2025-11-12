using Microsoft.EntityFrameworkCore;
using MindfulMe.API.Data;
using MindfulMe.API.DTOs;
using MindfulMe.API.Models;

namespace MindfulMe.API.Services
{
    public class JournalService : IJournalService
    {
        private readonly AppDbContext _context;

        public JournalService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<JournalEntry>> GetUserEntriesAsync(int userId)
        {
            return await _context.JournalEntries
                .Where(j => j.UserId == userId)
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync();
        }

        public async Task<JournalEntry?> GetEntryByIdAsync(int id)
        {
            return await _context.JournalEntries.FindAsync(id);
        }

        public async Task<JournalEntry> CreateEntryAsync(CreateJournalEntryRequest request)
        {
            var entry = new JournalEntry
            {
                UserId = request.UserId,
                EntryDate = DateTime.Now,
                Content = request.Content,
                MoodScore = request.MoodScore,
                StressLevel = request.StressLevel,
                HoursSlept = request.HoursSlept
            };

            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }

        public async Task<bool> UpdateEntryAsync(JournalEntry entry)
        {
            var existing = await _context.JournalEntries.FindAsync(entry.Id);
            if (existing == null || existing.UserId != entry.UserId)
                return false;

            existing.EntryDate = entry.EntryDate;
            existing.Content = entry.Content;
            existing.MoodScore = entry.MoodScore;
            existing.StressLevel = entry.StressLevel;
            existing.HoursSlept = entry.HoursSlept;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteEntryAsync(int id, int userId)
        {
            var entry = await _context.JournalEntries.FindAsync(id);
            if (entry == null || entry.UserId != userId)
                return false;

            _context.JournalEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}