using RFID_API.Data;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

// Configurar la cadena de conexión a SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));




// Agregar servicios necesarios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Agregar política CORS para permitir solicitudes desde Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy
            .WithOrigins("https://white-stone-0d90a691e.2.azurestaticapps.net")  // Dirección desde donde corre tu frontend Angular
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

// ✅ Usar CORS antes de Authorization
app.UseCors("AllowAngular");

app.UseAuthorization();

// Mapear los controladores (endpoints)
app.MapControllers();

// Ejecutar la aplicación
app.Run();
