using BlazorApp1.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServerSideBlazor();

builder.Services.AddRazorPages()
    .WithRazorPagesRoot("/Components/Pages");

builder.Services.AddAuthentication(options =>
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
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
    options.SaveTokens = true;
});

builder.Services.AddAuthorization();


var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5092/";

// Register HttpClients per service:
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

// 🔥 AND THIS IS THE ONE YOU MISSED 🔥  
builder.Services.AddHttpClient<MessageClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});


builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});


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
