using ClientesApi.Services;
using ClientesApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ---------- 1) Registrar los servicios----------
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IClienteStore, ClienteStore>();

// ---------- 2) Configurar cómo se atienden las peticiones ----------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// ---------- 3) Arrancar ----------
app.Run();

public partial class Program { }