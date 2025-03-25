using Microsoft.EntityFrameworkCore;
using testAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// �[�J Swagger �A��
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ū�� appsettings.json �����s�u�r��
builder.Services.AddDbContext<PowerDbContext>(options =>
    options.UseSqlServer(builder.Configuration["DefaultConnection"]));

// �[�J Controller �M JSON �R�W�]�w
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // �O�� JSON �榡���j�p�g
    });

var app = builder.Build();

// ���ެO�}�o�٬O���p���ҳ��ҥ� Swagger
app.UseSwagger();
app.UseSwaggerUI(); // �|�b /swagger ��ܤ���

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
