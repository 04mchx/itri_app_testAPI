using Microsoft.EntityFrameworkCore;
using testAPI.Data;

var builder = WebApplication.CreateBuilder(args);



// 讀取 appsettings.json 內的連線字串
builder.Services.AddDbContext<PowerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("itri_APP")));

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // 保持 JSON 格式的大小寫
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
