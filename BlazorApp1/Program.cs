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
// ───────────────────────── RAZOR / BLAZOR ─────────────────────────
// IMPORTANT: Tell Razor where your Pages actually are
//
builder.Services.AddRazorPages(options =>
{
    options.RootDirectory = "/Components/Pages";
});

builder.Services.AddServerSideBlazor();

//
// ───────────────────────── AUTHENTICATION ─────────────────────────
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
// ───────────────────────── API BASE URL ─────────────────────────
//
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
                 ?? "http://localhost:5092/";

//
// ───────────────────────── HTTP CLIENTS ─────────────────────────
//
builder.Services.AddHttpClient<HealthApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IDService, DService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IUService, UService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ICService, CService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IAService, AService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IMService, MService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IMealService, MealService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IMedicationService, MedicationService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IExerciseService, ExerciseService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IHydrationService, HydrationService>(c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddHttpClient<MessageClientService>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<ICalorieNinjaService, CalorieNinjaService>(client =>
{
    client.BaseAddress = new Uri("https://api.nal.usda.gov/fdc/v1/");
});

//
// ───────────────────────── APP BUILD ─────────────────────────
//
var app = builder.Build();

//
// ───────────────────────── MIDDLEWARE ─────────────────────────
//
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

//
// ───────────────────────── BLAZOR ROUTING ─────────────────────────
// THIS IS THE FIX
//
app.MapBlazorHub();
app.MapRazorPages();               // REQUIRED
app.MapFallbackToPage("/_Host");   // NOW IT WORKS

//
// ───────────────────────── AUTH ENDPOINTS ─────────────────────────
//
app.MapGet("/login-google", async (HttpContext context) =>
{
    await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = "/dashboard" });
});

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/");
});

app.Run();
