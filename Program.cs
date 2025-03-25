using Microsoft.EntityFrameworkCore;
using testAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// 加入 Swagger 服務
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 讀取 appsettings.json 內的連線字串
builder.Services.AddDbContext<PowerDbContext>(options =>
    options.UseSqlServer(builder.Configuration["DefaultConnection"]));

// 加入 Controller 和 JSON 命名設定
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // 保持 JSON 格式的大小寫
    });

var app = builder.Build();

// 不管是開發還是部署環境都啟用 Swagger
app.UseSwagger();
app.UseSwaggerUI(); // 會在 /swagger 顯示介面

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
