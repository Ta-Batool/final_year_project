using API.MongoModel;
using API.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using API.Hubs;                    // ✅ add this (namespace where CallHub lives)
using API.Ai;   // ⬅️ at the top of the file with the other using statements

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB settings
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<DoctorService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IMealService, MealService>();
builder.Services.AddSingleton<IMedicationService, MedicationService>();
builder.Services.AddSingleton<IConversationService, ConversationService>();
builder.Services.AddHttpClient<IAiAssistantService, AiAssistantService>();
builder.Services.AddSingleton<IExerciseService, ExerciseService>();
builder.Services.AddSingleton<IHydrationService, HydrationService>();



// ✅ SignalR for WebRTC signalling
builder.Services.AddSignalR();

// ✅ CORS so Blazor app can reach this API + SignalR hub
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy
            .WithOrigins(
                "https://fyp-blazor.onrender.com",  // your Render Blazor app
                "https://localhost:7090",           // local https dev
                "http://localhost:5090"             // local http dev (adjust if needed)
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ❌ On Render this can cause issues / warnings.
// app.UseHttpsRedirection();

app.UseRouting();

// ✅ Apply CORS before auth/endpoints
app.UseCors("AllowBlazor");

app.UseAuthorization();

// ✅ Simple root endpoint to test quickly
app.MapGet("/", () => Results.Ok("API is running"));

// ✅ Simple health endpoint that DOES NOT touch MongoDB
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

// ✅ Your actual API controllers
app.MapControllers();

// ✅ WebRTC signalling hub
app.MapHub<CallHub>("/callHub");

app.Run();
