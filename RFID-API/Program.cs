<<<<<<< HEAD
﻿using Microsoft.EntityFrameworkCore;
using RFID_API.Data;
using Microsoft.AspNetCore.SignalR;
using RFID_API.Hubs; // Asegúrate de tener este using si defines un Hub
=======
﻿using RFID_API.Data;
using Microsoft.EntityFrameworkCore;
using RFID_API.Hubs; // 👈 importa tu Hub
>>>>>>> b71045f101fd9673d0bdb68be404bbd4a2d684e7

var builder = WebApplication.CreateBuilder(args);

// 🔗 Conexión a SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

<<<<<<< HEAD
// 🔧 Controladores, Swagger y SignalR
=======
// Agregar servicios necesarios
>>>>>>> b71045f101fd9673d0bdb68be404bbd4a2d684e7
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR(); // ✅ Para SignalR

<<<<<<< HEAD
// 🔐 CORS para permitir acceso desde tu Angular Static Web App
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy
            .WithOrigins("https://white-stone-0d90a691e.2.azurestaticapps.net")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // ✅ Permitir credenciales (cookies, auth, etc.)
=======
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
>>>>>>> b71045f101fd9673d0bdb68be404bbd4a2d684e7
});

var app = builder.Build();

// 🧪 Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// ✅ Aplica CORS antes de cualquier middleware
app.UseCors("AllowAngular");

app.UseAuthorization();

<<<<<<< HEAD
// 🧭 Mapear tus controladores
app.MapControllers();

// 🔔 Mapear el Hub de SignalR (esto es lo nuevo)
app.MapHub<NotificationHub>("/hub/notificaciones");


=======
app.MapControllers();

// ✅ Mapea el SignalR hub
app.MapHub<NotificationHub>("/hub/notificaciones");

>>>>>>> b71045f101fd9673d0bdb68be404bbd4a2d684e7
app.Run();
