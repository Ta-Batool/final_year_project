using BlazorApp1.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Razor / Blazor
builder.Services.AddRazorPages()
    .WithRazorPagesRoot("/Components/Pages");

builder.Services.AddServerSideBlazor();

// 🔐 Authentication + Google OAuth
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/login-google";
        options.LogoutPath = "/logout";

        if (builder.Environment.IsDevelopment())
        {
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        }
        else
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        }

        options.Cookie.Name = ".FypAuth";
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
        options.CallbackPath = "/google-callback";

        options.SaveTokens = true;
        options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");

        if (builder.Environment.IsDevelopment())
        {
            options.CorrelationCookie.SameSite = SameSiteMode.Lax;
            options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        }
        else
        {
            options.CorrelationCookie.SameSite = SameSiteMode.None;
            options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        }
    });

builder.Services.AddAuthorization();

// 🌐 API base URL  (make sure this points to your API service on Render in production)
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5092/";

// ─── Domain services (typed HttpClients) ────────────────────────────────────────

builder.Services.AddHttpClient<IDService, DService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IUService, UService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<ICService, CService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IAService, AService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IMService, MService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IMealService, MealService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IMedicationHttpService, MedicationHttpService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IMedicationService, MedicationService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

// 📨 Chat / Messages API client  ✅ NEW
builder.Services.AddHttpClient<MessageClientService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});


// 🔁 TRANSLATION SERVICE (Google Cloud)
builder.Services.AddHttpClient<ITranslationService, TranslationService>();

// Fallback HttpClient (for any direct HttpClient injection)
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

builder.Services.AddHttpClient<ConversationClientService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<ICalorieNinjaService, CalorieNinjaService>(client =>
{
    client.BaseAddress = new Uri("https://api.api-ninjas.com/");

    var apiKey = builder.Configuration["CalorieNinjas:ApiKey"]; // from appsettings / env
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
    }
});




var app = builder.Build();

// Forwarded headers (Render / reverse proxy)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();

// 🔁 TRANSLATION API ENDPOINT
app.MapPost("/api/translate", async (TranslateRequest req, ITranslationService translator) =>
{
    var translated = await translator.TranslateAsync(req.TargetLanguage, req.Texts.ToArray());
    return Results.Ok(new { texts = translated });
});

app.MapFallbackToPage("/_Host");

// OAuth endpoints
app.MapGet("/login-google", async (HttpContext context) =>
{
    await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/google-callback"
    });
});

app.MapGet("/register-google", async (HttpContext httpContext) =>
{
    await httpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/dashboard?registered=true"
    });
});

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/");
});

app.Run();

public class TranslateRequest
{
    public string TargetLanguage { get; set; } = "ur";
    public List<string> Texts { get; set; } = new();
}
