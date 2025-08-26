using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Challenge_Qlik_Export.Services;
using Challenge_Qlik_Export.Models; // Ajoutez cette ligne

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure HttpClient
builder.Services.AddHttpClient();

// Register services
builder.Services.Configure<QlikSettings>(builder.Configuration.GetSection("QlikSettings"));
builder.Services.AddScoped<IQlikAuthService, QlikAuthService>();
builder.Services.AddScoped<IQlikExportService, QlikExportService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
// Ajoutez cette ligne pour injecter QlikSettings
builder.Services.Configure<QlikSettings>(builder.Configuration.GetSection("QlikSettings"));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        builder => builder
            .WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();