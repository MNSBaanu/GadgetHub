using Microsoft.EntityFrameworkCore;
using TechWorldAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Database connection
var conn = builder.Configuration.GetConnectionString("TechWorldDB");
builder.Services.AddDbContext<TechWorldDBContext>(options => 
{
    options.UseSqlServer(conn);
    options.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Repository services
builder.Services.AddScoped<ProductRepo>();
builder.Services.AddScoped<QuotationRepo>();

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGadgetHub", policy =>
    {
        policy.WithOrigins("https://localhost:7234", "http://localhost:7234", // GadgetHub Web App
                          "https://localhost:7091", "http://localhost:7091") // GadgetHub API
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowGadgetHub");

app.UseAuthorization();
app.MapControllers();

app.Run();
