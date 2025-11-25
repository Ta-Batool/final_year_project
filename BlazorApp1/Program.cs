using BlazorApp1.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// 🌍 Localization (for culture switching EN/UR)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

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

// 🌐 API base URL (used by all typed HttpClients)
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5092/";

// 🧵 Register HttpClients per service
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

// 📨 Messages service
builder.Services.AddHttpClient<IMService, MService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// 🥗 Meals service
builder.Services.AddHttpClient<IMealService, MealService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Fallback HttpClient (if something injects plain HttpClient)
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

var app = builder.Build();

// 🔁 Forwarded headers (for reverse proxies / Render / etc.)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// 🌍 Request localization (culture from cookie)
var supportedCultures = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("ur-PK")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
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
app.MapFallbackToPage("/_Host");

// 🔐 Google login
app.MapGet("/login-google", async (HttpContext context) =>
{
    await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/google-callback"
    });
});

// 🔐 Google register → after first login go to dashboard
app.MapGet("/register-google", async (HttpContext httpContext) =>
{
    await httpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/dashboard?registered=true"
    });
});

// 🚪 Logout: clear cookie + Google and redirect to home
app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignOutAsync(GoogleDefaults.AuthenticationScheme);

    context.Response.Redirect("/");
});

// 🌍 Language switch: /set-language/en-US or /set-language/ur-PK
app.MapGet("/set-language/{culture}", (string culture, HttpContext ctx) =>
{
    if (string.IsNullOrWhiteSpace(culture))
        culture = "en-US";

    var ci = new CultureInfo(culture);

    ctx.Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(ci)),
        new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1)
        });

    var referer = ctx.Request.Headers["Referer"].ToString();

    return Results.Redirect(string.IsNullOrEmpty(referer) ? "/" : referer);
});

app.Run();
