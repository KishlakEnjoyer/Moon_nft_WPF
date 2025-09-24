using Microsoft.EntityFrameworkCore;
using Moon_nft_api.Models;

var builder = WebApplication.CreateBuilder(args);

// ������������ DbContext
builder.Services.AddDbContext<MoonNftDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    // ��� ���, ���� ������ ������: new MySqlServerVersion(new Version(8, 0, 39))
    ));

// ��������� �����������
builder.Services.AddControllers();

// Swagger (���� ������������)
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