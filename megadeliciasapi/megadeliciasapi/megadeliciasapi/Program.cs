using megadeliciasapi.Data;
using megadeliciasapi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. DEFINE UNA VARIABLE PARA EL NOMBRE DE LA POLÍTICA ---
var MyCorsPolicy = "_myCorsPolicy";

// --- 2. AGREGA EL SERVICIO DE CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyCorsPolicy,
                      policy =>
                      {
                          // Permite que tu app de Angular (en localhost:4200) se conecte
                          policy.WithOrigins("http://localhost:4200")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// --- (Aquí va tu código existente de AddDbContext, AddAuthentication, etc.) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAuthentication(options =>
{
    //... (tu código de autenticación)
}).AddJwtBearer(options =>
{
    //... (tu código de JwtBearer)
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- (Aquí va tu código de Swagger) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- 3. USA LA POLÍTICA DE CORS ---
// (¡Debe ir ANTES de UseAuthorization y MapControllers!)
app.UseCors(MyCorsPolicy);

app.UseAuthentication(); // <-- Ya lo tienes
app.UseAuthorization();  // <-- Ya lo tienes
app.MapControllers();    // <-- Ya lo tienes

app.Run();