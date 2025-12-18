using API.Services;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly HealthLogService _health;
        public ReportsController(HealthLogService health) => _health = health;

        [HttpGet("health/pdf/{userId}")]
        public async Task<IActionResult> HealthPdf(string userId, [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var logs = await _health.GetRangeAsync(userId, from, to);
            var summary = await _health.GetSummaryAsync(userId, from, to);

            var bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Content().Column(col =>
                    {
                        col.Item().Text("Health Report").FontSize(20).Bold();
                        col.Item().Text($"UserId: {userId}");
                        col.Item().Text($"Range: {from:yyyy-MM-dd} to {to:yyyy-MM-dd}");
                        col.Item().LineHorizontal(1);

                        col.Item().Text("Summary").Bold();
                        col.Item().Text(System.Text.Json.JsonSerializer.Serialize(summary));

                        col.Item().LineHorizontal(1);
                        col.Item().Text("Logs").Bold();

                        foreach (var x in logs)
                        {
                            col.Item().Text($"{x.Timestamp:yyyy-MM-dd HH:mm} | BP {x.Systolic}/{x.Diastolic} | Glucose {x.Glucose} | W {x.WeightKg}kg | H {x.HeightCm}cm");
                        }
                    });
                });
            }).GeneratePdf();

            return File(bytes, "application/pdf", $"health-report-{userId}.pdf");
        }
    }
}
