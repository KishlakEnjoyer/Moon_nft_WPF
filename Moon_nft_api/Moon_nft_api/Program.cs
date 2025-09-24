using Microsoft.EntityFrameworkCore;
using Moon_nft_api.Models;

var builder = WebApplication.CreateBuilder(args);

// Регистрируем DbContext
builder.Services.AddDbContext<MoonNftDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    // ИЛИ так, если знаешь версию: new MySqlServerVersion(new Version(8, 0, 39))
    ));

// Добавляем контроллеры
builder.Services.AddControllers();

// Swagger (если используется)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();