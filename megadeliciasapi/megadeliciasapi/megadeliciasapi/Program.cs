using megadeliciasapi.Data; // <--- BIEN
using megadeliciasapi.Models;
using Microsoft.EntityFrameworkCore; // <--- BIEN

var builder = WebApplication.CreateBuilder(args);

// --- CONEXIÓN A BD (ESTO ESTÁ BIEN HECHO) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- SERVICIOS DE API ---
builder.Services.AddControllers(); // <-- ¡SOLO AddControllers(), NO AddControllersWithViews()!
builder.Services.AddEndpointsApiExplorer(); // (Para Swagger)
builder.Services.AddSwaggerGen(); // (Para Swagger)
var app = builder.Build();

// --- PIPELINE DE API ---
// Configura Swagger (la UI para probar tu API)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// (Dejamos esto listo para cuando configuremos JWT)
app.UseAuthorization();

// "MapControllers()" es para APIs (en lugar de MapControllerRoute)
app.MapControllers();

app.Run();