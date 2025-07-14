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

// ✅ Política CORS para permitir Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy
            .WithOrigins("http://localhost:4200") // o el dominio de tu frontend si ya lo subiste
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// ✅ IMPORTANTE para Railway: usar el puerto de entorno
var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Mostrar Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // ❌ NO redirijas a HTTPS en producción (Railway ya usa HTTPS)
    // app.UseHttpsRedirection();  // Se comenta para evitar error 502
}

// ✅ Aplicar CORS antes de Authorization
app.UseCors("AllowAngular");

app.UseAuthorization();

// Ruta base opcional para prueba
app.MapGet("/", () => "✅ API RFID en funcionamiento");

// Mapear los controladores
app.MapControllers();

app.Run();
