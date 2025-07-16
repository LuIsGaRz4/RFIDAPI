using RFID_API.Data;
using Microsoft.EntityFrameworkCore;
using RFID_API.Hubs; // 👈 importa tu Hub

var builder = WebApplication.CreateBuilder(args);

// 🔗 Conexión a SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Agregar servicios necesarios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR(); // ✅ Para SignalR

// ✅ Agrega SignalR
builder.Services.AddSignalR();

// ✅ Agregar política CORS para permitir solicitudes desde Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy
            .WithOrigins("https://white-stone-0d90a691e.2.azurestaticapps.net")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // 👈 necesario para SignalR con WebSockets
});

var app = builder.Build();

// 🧪 Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// ✅ Aplica CORS antes de cualquier middleware
app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

// ✅ Mapea el SignalR hub
app.MapHub<NotificationHub>("/hub/notificaciones");

app.Run();
