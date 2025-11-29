using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

[ApiController]
[Route("api/test-calorie")]
public class TestCalorieController : ControllerBase
{
    private readonly IConfiguration _config;

    public TestCalorieController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("nutrition")]
    public async Task<IActionResult> TestNutrition([FromQuery] string query = "egg")
    {
        var apiKey = _config["CalorieNinjas:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return BadRequest(new { error = "API key is missing in configuration." });
        }

        using var http = new HttpClient();
        var url = $"https://api.calorieninjas.com/v1/nutrition?query={query}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Add("X-Api-Key", apiKey);

        var response = await http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        return Ok(new
        {
            status = response.StatusCode.ToString(),
            raw = body
        });
    }
}
