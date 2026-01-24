using Microsoft.EntityFrameworkCore;
using Coursework.Models;
using Coursework.Data;

namespace Coursework.Services
{
    public class DatabaseService
    {
        private readonly AppDbContext _context;

        public DatabaseService(AppDbContext context)
        {
            _context = context;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            // Ensuring that the database is created
            _context.Database.EnsureCreated();
        }

        // For Journal Entry Methods
        public List<JournalEntry> GetAllJournalEntries()
        {
            return _context.JournalEntries
                .OrderByDescending(e => e.CreatedAt)
                .ToList();
        }

        public JournalEntry? GetJournalEntry(int id)
        {
            return _context.JournalEntries
                .FirstOrDefault(e => e.Id == id);
        }

        public JournalEntry? GetJournalEntryByDate(DateTime date)
        {
            var dateOnly = date.Date;
            return _context.JournalEntries
                .Where(e => e.CreatedAt.Date == dateOnly)
                .FirstOrDefault();
        }

        public List<DateTime> GetMissedDays(DateTime startDate, DateTime endDate)
        {
            var entries = GetJournalEntriesByDateRange(startDate, endDate);
            var entryDates = entries.Select(e => e.CreatedAt.Date).Distinct().ToHashSet();
            
            var missedDays = new List<DateTime>();
            var currentDate = startDate.Date;
            
            while (currentDate <= endDate.Date)
            {
                if (!entryDates.Contains(currentDate))
                {
                    missedDays.Add(currentDate);
                }
                currentDate = currentDate.AddDays(1);
            }
            
            return missedDays;
        }

        public string GetMostFrequentMood(int days = 30)
        {
            var distribution = GetMoodDistribution(days);
            if (distribution.Count == 0) return "None";
            
            return distribution.OrderByDescending(m => m.Value).First().Key;
        }

        public Dictionary<string, int> GetTagBreakdownByCategory()
        {
            var entries = GetAllJournalEntries();
            var categoryTagCounts = new Dictionary<string, Dictionary<string, int>>();
            
            foreach (var entry in entries)
            {
                var category = string.IsNullOrEmpty(entry.Category) ? "Uncategorized" : entry.Category;
                
                if (!categoryTagCounts.ContainsKey(category))
                {
                    categoryTagCounts[category] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                }
                
                if (!string.IsNullOrEmpty(entry.Tags))
                {
                    var tags = entry.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (var tag in tags)
                    {
                        categoryTagCounts[category][tag] = categoryTagCounts[category].GetValueOrDefault(tag, 0) + 1;
                    }
                }
            }
            
            // Flatten to show total tag usage per category
            var result = new Dictionary<string, int>();
            foreach (var category in categoryTagCounts.Keys)
            {
                result[category] = categoryTagCounts[category].Values.Sum();
            }
            
            return result;
        }

        public JournalEntry SaveJournalEntry(JournalEntry entry)
        {
            if (entry.Id == 0)
            {
                entry.CreatedAt = DateTime.Now;
                entry.UpdatedAt = DateTime.Now;
                _context.JournalEntries.Add(entry);
            }
            else
            {
                entry.UpdatedAt = DateTime.Now;
                _context.JournalEntries.Update(entry);
            }
            
            _context.SaveChanges();
            
            // Reload the entry to ensure we have the latest database values
            return _context.JournalEntries.Find(entry.Id)!;
        }

        public int DeleteJournalEntry(int id)
        {
            var entry = _context.JournalEntries.Find(id);
            if (entry != null)
            {
                _context.JournalEntries.Remove(entry);
                _context.SaveChanges();
                return 1;
            }
            return 0;
        }

        public List<JournalEntry> GetJournalEntriesByDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.JournalEntries
                .Where(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate)
                .OrderByDescending(e => e.CreatedAt)
                .ToList();
        }

        public List<JournalEntry> GetFavoriteJournalEntries()
        {
            return _context.JournalEntries
                .Where(e => e.IsFavorite)
                .OrderByDescending(e => e.CreatedAt)
                .ToList();
        }

        // User Methods
        public User? GetUserByUsername(string username)
        {
            return _context.Users
                .FirstOrDefault(u => u.Username == username);
        }

        public User? GetUserByEmail(string email)
        {
            return _context.Users
                .FirstOrDefault(u => u.Email == email);
        }

        public int SaveUser(User user)
        {
            if (user.Id == 0)
            {
                user.CreatedAt = DateTime.Now;
                _context.Users.Add(user);
            }
            else
            {
                _context.Users.Update(user);
            }
            
            _context.SaveChanges();
            return user.Id;
        }

        public bool ValidateUser(string username, string passwordHash)
        {
            var user = GetUserByUsername(username);
            return user != null && user.PasswordHash == passwordHash;
        }

        // Statistics Methods
        public int GetTotalJournalEntries()
        {
            return _context.JournalEntries.Count();
        }

        public int GetStreakDays()
        {
            var entries = GetAllJournalEntries();
            if (entries.Count == 0) return 0;

            int streak = 0;
            DateTime currentDate = DateTime.Now.Date;
            
            foreach (var entry in entries.OrderByDescending(e => e.CreatedAt.Date))
            {
                if (entry.CreatedAt.Date == currentDate)
                {
                    streak++;
                    currentDate = currentDate.AddDays(-1);
                }
                else if (entry.CreatedAt.Date < currentDate)
                {
                    break;
                }
            }
            
            return streak;
        }

        public int GetLongestStreak()
        {
            var entries = GetAllJournalEntries();
            if (entries.Count == 0) return 0;

            var dates = entries.Select(e => e.CreatedAt.Date).Distinct().OrderByDescending(d => d).ToList();
            if (dates.Count == 0) return 0;

            int longestStreak = 1;
            int currentStreak = 1;

            for (int i = 1; i < dates.Count; i++)
            {
                if ((dates[i - 1] - dates[i]).Days == 1)
                {
                    currentStreak++;
                    longestStreak = Math.Max(longestStreak, currentStreak);
                }
                else
                {
                    currentStreak = 1;
                }
            }

            return longestStreak;
        }

        public Dictionary<string, int> GetMoodDistribution(int days = 30)
        {
            var startDate = DateTime.Now.AddDays(-days);
            var entries = GetJournalEntriesByDateRange(startDate, DateTime.Now);
            
            var moodCounts = new Dictionary<string, int>();
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.PrimaryMood))
                {
                    moodCounts[entry.PrimaryMood] = moodCounts.GetValueOrDefault(entry.PrimaryMood, 0) + 1;
                }
            }
            
            return moodCounts;
        }

        public Dictionary<string, int> GetTopTags(int limit = 5)
        {
            var entries = GetAllJournalEntries();
            var tagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.Tags))
                {
                    var tags = entry.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (var tag in tags)
                    {
                        tagCounts[tag] = tagCounts.GetValueOrDefault(tag, 0) + 1;
                    }
                }
            }
            
            return tagCounts.OrderByDescending(t => t.Value)
                           .Take(limit)
                           .ToDictionary(t => t.Key, t => t.Value);
        }

        public double GetAverageWordCount()
        {
            var entries = GetAllJournalEntries();
            if (entries.Count == 0) return 0;

            int totalWords = 0;
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.Content))
                {
                    var plainText = System.Text.RegularExpressions.Regex.Replace(entry.Content, "<.*?>", " ");
                    var words = plainText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    totalWords += words.Length;
                }
            }

            return entries.Count > 0 ? (double)totalWords / entries.Count : 0;
        }

        // Search and Filter Methods
        public List<JournalEntry> SearchEntries(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllJournalEntries();

            var term = searchTerm.ToLower();
            return _context.JournalEntries
                .Where(e => 
                    e.Title.ToLower().Contains(term) || 
                    e.Content.ToLower().Contains(term))
                .OrderByDescending(e => e.CreatedAt)
                .ToList();
        }

        public List<JournalEntry> FilterEntries(
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            string? mood = null, 
            string? tag = null)
        {
            var query = _context.JournalEntries.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedAt >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedAt <= endDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            if (!string.IsNullOrWhiteSpace(mood))
            {
                query = query.Where(e => e.PrimaryMood == mood || e.SecondaryMoods.Contains(mood));
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(e => e.Tags.Contains(tag, StringComparison.OrdinalIgnoreCase));
            }

            return query.OrderByDescending(e => e.CreatedAt).ToList();
        }

        public List<JournalEntry> SearchAndFilterEntries(
            string? searchTerm = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? mood = null,
            string? tag = null)
        {
            var query = _context.JournalEntries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(e => 
                    e.Title.ToLower().Contains(term) || 
                    e.Content.ToLower().Contains(term));
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedAt >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedAt <= endDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            if (!string.IsNullOrWhiteSpace(mood))
            {
                query = query.Where(e => e.PrimaryMood == mood || e.SecondaryMoods.Contains(mood));
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                query = query.Where(e => e.Tags.Contains(tag, StringComparison.OrdinalIgnoreCase));
            }

            return query.OrderByDescending(e => e.CreatedAt).ToList();
        }

        public Dictionary<DateTime, int> GetEntriesByDate(DateTime month)
        {
            var startDate = new DateTime(month.Year, month.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var entries = GetJournalEntriesByDateRange(startDate, endDate);
            
            return entries
                .GroupBy(e => e.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
