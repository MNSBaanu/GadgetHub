using Microsoft.EntityFrameworkCore;
using GadgetHubAPI.Data;
using GadgetHubAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Database connection
var conn = builder.Configuration.GetConnectionString("GadgetHubDB");
builder.Services.AddDbContext<GadgetHubDBContext>(options => options.UseSqlServer(conn));

// Repository services
builder.Services.AddScoped<ProductRepo>();
builder.Services.AddScoped<CustomerRepo>();
builder.Services.AddScoped<OrderRepo>();
builder.Services.AddScoped<QuotationComparisonRepo>();

// HTTP client for distributor services with SSL certificate handling
builder.Services.AddHttpClient<DistributorService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "GadgetHub-API/1.0");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
{
    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
});

builder.Services.AddHttpClient<OrderProcessingService>();
builder.Services.AddHttpClient<ProductService>();

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Business services
builder.Services.AddScoped<DistributorService>();
builder.Services.AddScoped<OrderProcessingService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<NotificationService>();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();
