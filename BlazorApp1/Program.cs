using System;
using System.Collections.Generic;
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

//
// ─────────────────────────────────────────────────────────────
// Razor + Blazor Server
// ─────────────────────────────────────────────────────────────
//
builder.Services.AddRazorPages()
    .WithRazorPagesRoot("/Components/Pages");

builder.Services
    .AddServerSideBlazor()
    .AddCircuitOptions(o =>
    {
        o.DetailedErrors = true;
    });

//
// ─────────────────────────────────────────────────────────────
// 🔐 Authentication (Cookies + Google OAuth)
// ─────────────────────────────────────────────────────────────
//
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

//
// ─────────────────────────────────────────────────────────────
// 🌐 API BASE URL
// ─────────────────────────────────────────────────────────────
//
var apiBaseUrl =
    builder.Configuration["ApiBaseUrl"]
    ?? "http://localhost:5092/"; // local backend

//
// ─────────────────────────────────────────────────────────────
// ✅ Named fallback HttpClient (keep this)
// ─────────────────────────────────────────────────────────────
//
builder.Services.AddHttpClient("Api", c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));

//
// ─────────────────────────────────────────────────────────────
// ✅ Typed HttpClients (your HTTP API services)
// ─────────────────────────────────────────────────────────────
//
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

//
// ─────────────────────────────────────────────────────────────
// ✅ NON-HTTP SERVICES (pure logic / orchestrators)
// ─────────────────────────────────────────────────────────────
//

// If MetabolismApiService uses IUService/IMealService/IExerciseService internally,
// it should be AddScoped (NOT AddHttpClient).
builder.Services.AddScoped<MetabolismApiService>();

// ✅ FIXED: ExercisePlanService is NOT HttpClient-based -> AddScoped
builder.Services.AddScoped<IExercisePlanService, ExercisePlanService>();

builder.Services.AddScoped<AppointmentApiService>();
builder.Services.AddScoped<DoctorPatientsApiService>();
builder.Services.AddScoped<ExerciseApiClient>();


builder.Services.AddHttpClient<IDoctorVerificationApiService, DoctorVerificationApiService>(
    c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddHttpClient<IDoctorVerificationApiService, DoctorVerificationApiService>(
    c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddHttpClient<IAdminApiService, AdminApiService>(
    c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddScoped<AdminSession>();
builder.Services.AddScoped<AdminApiClient>();

//
// ─────────────────────────────────────────────────────────────
// 🧾 External APIs
// ─────────────────────────────────────────────────────────────
//
builder.Services.AddHttpClient<ITranslationService, TranslationService>();

builder.Services.AddHttpClient<ICalorieNinjaService, CalorieNinjaService>(c =>
{
    c.BaseAddress = new Uri("https://api.nal.usda.gov/fdc/v1/");
});

//
// ─────────────────────────────────────────────────────────────
// 🚀 APP PIPELINE
// ─────────────────────────────────────────────────────────────
//
var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
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

//
// ─────────────────────────────────────────────────────────────
// 🌍 Minimal API Endpoints
// ─────────────────────────────────────────────────────────────
//
app.MapPost("/api/translate", async (TranslateRequest req, ITranslationService translator) =>
{
    var translated = await translator.TranslateAsync(req.TargetLanguage, req.Texts.ToArray());
    return Results.Ok(new { texts = translated });
});

//
// ─────────────────────────────────────────────────────────────
// OAuth endpoints
// ─────────────────────────────────────────────────────────────
//
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
        new AuthenticationProperties { RedirectUri = "/dashboard?registered=true" });
});

app.MapGet("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    ctx.Response.Redirect("/");
});

app.MapFallbackToPage("/_Host");
app.Run();

public class TranslateRequest
{
    public string TargetLanguage { get; set; } = "ur";
    public List<string> Texts { get; set; } = new();
}
