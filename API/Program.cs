using API.MongoModel;
using API.Services;

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ❌ On Render this can cause issues / warnings.
//    You don't really need HTTPS redirection inside the container.
// app.UseHttpsRedirection();

app.UseAuthorization();

// ✅ Simple root endpoint to test quickly
app.MapGet("/", () => Results.Ok("API is running"));

// ✅ Simple health endpoint that DOES NOT touch MongoDB
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

// ✅ Your actual API controllers
app.MapControllers();

app.Run();
