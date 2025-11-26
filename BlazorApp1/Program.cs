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

// 🌐 API base URL
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5092/";

// Domain services
builder.Services.AddHttpClient<IDService, DService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IUService, UService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ICService, CService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IAService, AService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IMService, MService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IMealService, MealService>(c => c.BaseAddress = new Uri(apiBaseUrl));

// ⬇️ NEW: Medication API client
builder.Services.AddHttpClient<IMedicationService, MedicationService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

// 🔁 TRANSLATION SERVICE (Google Cloud)
builder.Services.AddHttpClient<ITranslationService, TranslationService>();

// Fallback HttpClient
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

var app = builder.Build();

// Forwarded headers (Render)
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
