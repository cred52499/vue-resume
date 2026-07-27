using backend;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5286");

// 1. 註冊 CORS (允許 Vue 存取)
var allowedOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>() 
                     ?? new[] { "http://192.168.200.171:5173/vue-resume/" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "https://cred52499.github.io" // GitHub Pages 網址
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// 2. 註冊 MySQL 資料庫連線
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowVueApp");

// POST: 新增訪問記錄並回傳總次數
app.MapPost("/api/visit", async (AppDbContext db) =>
{
    db.VisitLogs.Add(new VisitLog());
    await db.SaveChangesAsync();

    var totalVisits = await db.VisitLogs.CountAsync();
    return Results.Ok(new { message = "訪問成功", totalVisits = totalVisits });
});

// GET: 純讀取總次數，不新增資料庫記錄 (供 F5 重新整理使用)
app.MapGet("/api/visit", async (AppDbContext db) =>
{
    var totalVisits = await db.VisitLogs.CountAsync();
    return Results.Ok(new { totalVisits = totalVisits });
});

app.Run();