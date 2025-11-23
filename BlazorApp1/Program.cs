using System;
using System.Net.Http;
using BlazorApp1.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// Blazor & Razor Pages
// ----------------------
builder.Services.AddRazorPages()
    .WithRazorPagesRoot("/Components/Pages");

builder.Services.AddServerSideBlazor();

// ----------------------
// Authentication & Google OAuth
// ----------------------
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
    })
    .AddGoogle(options =>
    {
        // Values come from appsettings.json or environment variables:
        // Authentication__Google__ClientId
        // Authentication__Google__ClientSecret
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";

        // We keep the default internal callback (/signin-google).
        // After that completes, user is redirected to RedirectUri
        // that we specify in the Challenge (see /login-google).
        options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
        options.SaveTokens = true;
    });

builder.Services.AddAuthorization();

// ----------------------
// API HttpClient setup
// ----------------------
// In production, set ApiBaseUrl as an environment variable.
// e.g. ApiBaseUrl = https://your-api-service.onrender.com/
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? throw new Exception("API base URL missing!");


builder.Services.AddHttpClient<IDService, DService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IUService, UService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<ICService, CService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IAService, AService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IMService, MService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<MessageClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Fallback HttpClient (if something injects just HttpClient)
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

// ----------------------
// Build app
// ----------------------
var app = builder.Build();

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
app.MapFallbackToPage("/_Host");

// ----------------------
// OAuth endpoints
// ----------------------

// Start Google login
app.MapGet("/login-google", async (HttpContext context) =>
{
    await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        // After Google + internal /signin-google finish,
        // user will be redirected here:
        RedirectUri = "/google-callback"
    });
});

// Separate entry if you ever want a distinct registration flow
app.MapGet("/register-google", async (HttpContext context) =>
{
    await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/dashboard?registered=true"
    });
});

// This is called AFTER the Google handler has signed the user in.
// At this point, the user should already be authenticated via the cookie.
app.MapGet("/google-callback", (HttpContext context) =>
{
    var isAuthed = context.User?.Identity?.IsAuthenticated ?? false;

    if (!isAuthed)
    {
        return Results.Redirect("/login?error=google-auth-failed");
    }

    // Go to your dashboard (or wherever you want)
    return Results.Redirect("/dashboard");
});

// Logout
app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/");
});

app.Run();
