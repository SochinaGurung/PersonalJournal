using Coursework.Models;
using System.Text;

namespace Coursework.Services
{
    public class PdfExportService
    {
        private readonly DatabaseService _databaseService;

        public PdfExportService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        private string HtmlEncode(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }

        public string GenerateHtmlForPdf(List<JournalEntry> entries, DateTime startDate, DateTime endDate)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; padding: 20px; }");
            html.AppendLine("h1 { color: #333; border-bottom: 2px solid #333; padding-bottom: 10px; }");
            html.AppendLine(".entry { margin-bottom: 30px; page-break-inside: avoid; border: 1px solid #ddd; padding: 15px; border-radius: 5px; }");
            html.AppendLine(".entry-header { display: flex; justify-content: space-between; margin-bottom: 10px; }");
            html.AppendLine(".entry-title { font-size: 18px; font-weight: bold; color: #333; }");
            html.AppendLine(".entry-date { color: #666; font-size: 12px; }");
            html.AppendLine(".entry-meta { margin: 10px 0; font-size: 12px; color: #666; }");
            html.AppendLine(".entry-content { margin-top: 10px; line-height: 1.6; }");
            html.AppendLine(".tags { margin-top: 10px; }");
            html.AppendLine(".tag { display: inline-block; background: #f0f0f0; padding: 3px 8px; margin: 2px; border-radius: 3px; font-size: 11px; }");
            html.AppendLine("@media print { .entry { page-break-inside: avoid; } }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");
            html.AppendLine($"<h1>Journal Entries</h1>");
            html.AppendLine($"<p>Date Range: {startDate:MMMM dd, yyyy} to {endDate:MMMM dd, yyyy}</p>");
            html.AppendLine($"<p>Total Entries: {entries.Count}</p>");
            html.AppendLine("<hr>");

            foreach (var entry in entries.OrderByDescending(e => e.CreatedAt))
            {
                html.AppendLine("<div class='entry'>");
                html.AppendLine("<div class='entry-header'>");
                html.AppendLine($"<div class='entry-title'>{HtmlEncode(entry.Title)}</div>");
                html.AppendLine($"<div class='entry-date'>{entry.CreatedAt:MMMM dd, yyyy 'at' hh:mm tt}</div>");
                html.AppendLine("</div>");
                
                html.AppendLine("<div class='entry-meta'>");
                html.AppendLine($"<strong>Mood:</strong> {HtmlEncode(entry.PrimaryMood)}");
                if (!string.IsNullOrEmpty(entry.SecondaryMoods))
                {
                    html.AppendLine($" | <strong>Secondary:</strong> {HtmlEncode(entry.SecondaryMoods)}");
                }
                html.AppendLine($" | <strong>Category:</strong> {HtmlEncode(entry.Category)}");
                html.AppendLine("</div>");
                
                html.AppendLine("<div class='entry-content'>");
                html.AppendLine(entry.Content); // HTML content from Quill
                html.AppendLine("</div>");
                
                if (!string.IsNullOrEmpty(entry.Tags))
                {
                    html.AppendLine("<div class='tags'>");
                    var tags = entry.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (var tag in tags)
                    {
                        html.AppendLine($"<span class='tag'>#{HtmlEncode(tag)}</span>");
                    }
                    html.AppendLine("</div>");
                }
                
                html.AppendLine("</div>");
            }

            html.AppendLine("</body></html>");
            return html.ToString();
        }

        public string GenerateHtmlForPdfExport(List<JournalEntry> entries, DateTime startDate, DateTime endDate)
        {
            return GenerateHtmlForPdf(entries, startDate, endDate);
        }
    }
}
