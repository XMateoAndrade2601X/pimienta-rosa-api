using Microsoft.EntityFrameworkCore;
using PimientaRosa.API.Data;
using PimientaRosa.API.Hubs;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// Base de datos
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//SignalR
builder.Services.AddSignalR();


// CORS para Lovable
builder.Services.AddCors(options =>
{
    options.AddPolicy("Lovable", policy =>
        policy.WithOrigins("https://pimienta-rosa-delivery-campus.lovable.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()); // 👈 importante para SignalR
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("Lovable");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<PedidoHub>("/hubs/pedido");
app.Run();