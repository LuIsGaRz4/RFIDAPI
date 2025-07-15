using Microsoft.EntityFrameworkCore;
using RFID_API.Data;
using Microsoft.AspNetCore.SignalR;
using RFID_API.Hubs; // Asegúrate de tener este using si defines un Hub

var builder = WebApplication.CreateBuilder(args);

// 🔗 Conexión a SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔧 Controladores, Swagger y SignalR
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR(); // ✅ Para SignalR

// 🔐 CORS para permitir acceso desde tu Angular Static Web App
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy
            .WithOrigins("https://white-stone-0d90a691e.2.azurestaticapps.net")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // ✅ Permitir credenciales (cookies, auth, etc.)
});

var app = builder.Build();

// 🧪 Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// ✅ Aplica CORS antes de cualquier middleware
app.UseCors("AllowAngular");

app.UseAuthorization();

// 🧭 Mapear tus controladores
app.MapControllers();

// 🔔 Mapear el Hub de SignalR (esto es lo nuevo)
app.MapHub<NotificationHub>("/hub/notificaciones");


app.Run();
