using API.MongoModel;
using API.Services;
using MongoDB.Driver;
using API.Hubs;
using API.Ai;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddSingleton<DoctorPatientService>();
builder.Services.AddSingleton<HealthLogService>();
builder.Services.AddSingleton<DietPlanService>();
builder.Services.AddSingleton<ExercisePlanService>();
builder.Services.AddSingleton<DailyChecklistService>();
builder.Services.AddHttpClient<ICaloriesBurnedApiService, CaloriesBurnedApiService>();
builder.Services.AddSingleton<BPLogService>();
builder.Services.AddSingleton<GlucoseLogService>();
builder.Services.AddSingleton<WeightLogService>();
builder.Services.Configure<API.Otp.OtpSettings>(
    builder.Configuration.GetSection("Otp"));
builder.Services.AddSingleton<API.Services.OtpService>();

builder.Services.AddScoped<API.Services.ExercisePlanService>();

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
                "http://localhost:5090"
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
app.UseAuthorization();

app.MapGet("/", () => Results.Ok("API is running"));
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapControllers();
app.MapHub<CallHub>("/callHub");

app.Run();
