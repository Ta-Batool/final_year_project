using System;
using System.Collections.Generic;
using System.IO;
using BlazorApp1.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// =======================
// Load .env (local dev)
// =======================
DotNetEnv.Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
DotNetEnv.Env.Load(Path.Combine(Directory.GetCurrentDirectory(), "BlazorApp1", ".env"));

var builder = WebApplication.CreateBuilder(args);

// Allow env vars to override appsettings
builder.Configuration.AddEnvironmentVariables();

// Razor Pages + Blazor Server
builder.Services.AddRazorPages()
    .WithRazorPagesRoot("/Components/Pages");

builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(o => o.DetailedErrors = true);

// =======================
// Cookie Policy (FIXED)
// =======================
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    options.OnAppendCookie = ctx => FixSameSite(ctx.CookieOptions);
    options.OnDeleteCookie = ctx => FixSameSite(ctx.CookieOptions);
});

static void FixSameSite(CookieOptions options)
{
    if (options.SameSite == SameSiteMode.None)
    {
        options.Secure = true;
    }
}

// =======================
// Authentication
// =======================
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
        options.Cookie.Name = ".FypAuth";
        options.AccessDeniedPath = "/access-denied";

        // Main cookie
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
        options.Cookie.Path = "/";
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";

        // ✅ FIXED: separate internal callback
        options.CallbackPath = "/signin-google";

        options.SaveTokens = true;

        options.Scope.Add("email");
        options.Scope.Add("profile");

        options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");

        // Correlation cookie fix
        options.CorrelationCookie.SameSite = SameSiteMode.None;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.CorrelationCookie.HttpOnly = true;
        options.CorrelationCookie.Path = "/";
    });

builder.Services.AddAuthorization();

// =======================
// API base url
// =======================
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5092/";

// Base HttpClient
builder.Services.AddHttpClient("Api", c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));

// Typed HTTP services
builder.Services.AddHttpClient<HealthApiService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IDService, DService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IUService, UService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ICService, CService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IAService, AService>(c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddHttpClient<IMService, MService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IMealService, MealService>(c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddHttpClient<IExerciseService, ExerciseService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IHydrationService, HydrationService>(c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddHttpClient<IMedicationHttpService, MedicationHttpService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IMedicationService, MedicationService>(c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddHttpClient<MessageClientService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ConversationClientService>(c => c.BaseAddress = new Uri(apiBaseUrl));

// Non-HTTP services
builder.Services.AddScoped<MetabolismApiService>();
builder.Services.AddScoped<IExercisePlanService, ExercisePlanService>();
builder.Services.AddScoped<AppointmentApiService>();
builder.Services.AddScoped<DoctorPatientsApiService>();
builder.Services.AddScoped<ExerciseApiClient>();

builder.Services.AddHttpClient<IDoctorVerificationApiService, DoctorVerificationApiService>(
    c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddHttpClient<IAdminApiService, AdminApiService>(
    c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<AdminSession>();

builder.Services.AddHttpClient<ITranslationService, TranslationService>();
builder.Services.AddHttpClient<ICalorieNinjaService, CalorieNinjaService>(c =>
{
    c.BaseAddress = new Uri("https://api.nal.usda.gov/fdc/v1/");
});

// =======================
// Pipeline
// =======================
var app = builder.Build();

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

// VERY IMPORTANT
app.UseCookiePolicy();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();

// API endpoint
app.MapPost("/api/translate", async (TranslateRequest req, ITranslationService translator) =>
{
    var translated = await translator.TranslateAsync(req.TargetLanguage, req.Texts.ToArray());
    return Results.Ok(new { texts = translated });
});

// =======================
// OAuth Endpoints
// =======================
app.MapGet("/login-google", async (HttpContext ctx) =>
{
    await ctx.ChallengeAsync(
        GoogleDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = "/google-callback" });
});

app.MapGet("/register-google", async (HttpContext ctx) =>
{
    await ctx.ChallengeAsync(
        GoogleDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = "/google-callback" });
});

app.MapGet("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    ctx.Response.Redirect("/");
});

app.MapFallbackToPage("/_Host");
app.Run();

// =======================
// DTO
// =======================
public class TranslateRequest
{
    public string TargetLanguage { get; set; } = "ur";
    public List<string> Texts { get; set; } = new();
}