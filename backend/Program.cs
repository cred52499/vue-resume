var builder = WebApplication.CreateBuilder(args);

// 1. 註冊 CORS 服務（允許你的 Vue 前端存取 API）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vue 的預設網址
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 套用 CORS 政策
app.UseCors("AllowVueApp");

// --------------------------------------------------
// 你的自訂 API 請寫在這裡下方！
// --------------------------------------------------

// 範例：你可以寫一個簡單的測試 API 來驗證連線
app.MapGet("/api/test", () => new { message = "後端成功連線！" });

app.Run();