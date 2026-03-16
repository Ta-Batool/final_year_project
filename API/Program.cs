using System.IO;
using API.MongoModel;
using API.Services;
using MongoDB.Driver;
using API.Hubs;
using API.Ai;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// ✅ Load .env (LOCAL ONLY). Put your .env in API/.env
DotNetEnv.Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
DotNetEnv.Env.Load(Path.Combine(Directory.GetCurrentDirectory(), "API", ".env"));

// ✅ Allow environment variables (and .env via DotNetEnv) to override appsettings
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✅ Swagger (FIX multipart/form-data + IFormFile)
builder.Services.AddSwaggerGen(c =>
{
    // Fix file uploads with [FromForm] IFormFile
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });

    // Also adjust requestBody schema for multipart endpoints
    c.OperationFilter<API.Swagger.MultipartFormOperationFilter>();
});

// ✅ Bind Mongo settings
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));

// ✅ Register Mongo Client + Database for DI
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;

    if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        throw new Exception("MongoDB ConnectionString is missing. Set MongoDB:ConnectionString or MongoDB__ConnectionString in .env");

    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;

    if (string.IsNullOrWhiteSpace(settings.DatabaseName))
        throw new Exception("MongoDB DatabaseName is missing. Set MongoDB:DatabaseName or MongoDB__DatabaseName in .env");

    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

// ✅ Existing services
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
builder.Services.AddSingleton<DoctorPatientService>();
builder.Services.AddSingleton<HealthLogService>();
builder.Services.AddSingleton<DietPlanService>();

// ✅ Keep only ONE ExercisePlanService registration
builder.Services.AddSingleton<ExercisePlanService>();

builder.Services.AddSingleton<DailyChecklistService>();
builder.Services.AddHttpClient<ICaloriesBurnedApiService, CaloriesBurnedApiService>();
builder.Services.AddSingleton<BPLogService>();
builder.Services.AddSingleton<GlucoseLogService>();
builder.Services.AddSingleton<WeightLogService>();

// ✅ New premium / coach / workouts services
builder.Services.AddSingleton<CheckInService>();
builder.Services.AddSingleton<FitnessCoachService>();
builder.Services.AddSingleton<PaymentService>();
builder.Services.AddSingleton<WorkoutService>();

builder.Services.Configure<API.Otp.OtpSettings>(builder.Configuration.GetSection("Otp"));
builder.Services.AddSingleton<API.Services.OtpService>();

// ✅ Admin (Basic Auth) for /api/admin endpoints
builder.Services.AddSingleton<API.Security.AdminAuth>();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy
            .WithOrigins(
                "https://fyp-blazor.onrender.com",
                "https://localhost:7090",
                "http://localhost:5090",
                "https://localhost:7126",
                "http://localhost:7126"
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

app.UseRouting();
app.UseCors("AllowBlazor");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok("API is running"));
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapControllers();
app.MapHub<CallHub>("/callHub");

app.Run();
