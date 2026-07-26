using GadgetHubAPI.Configuration;
using GadgetHubAPI.Data;
using GadgetHubAPI.Services;
using GadgetHubWeb.Services;
using Microsoft.EntityFrameworkCore;
using ApiDbContext = GadgetHubAPI.Data.GadgetHubDBContext;
using WebDbContext = GadgetHubWeb.Data.GadgetHubDBContext;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

builder.Services.Configure<DistributorUrls>(
    builder.Configuration.GetSection(DistributorUrls.SectionName));

var conn = builder.Configuration.GetConnectionString("GadgetHubDB");

// Web UI context (existing pages / AuthService)
builder.Services.AddDbContext<WebDbContext>(options =>
    options.UseSqlServer(conn));

// Embedded API context (orders, quotations, products from API controllers)
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(conn));

builder.Services.AddScoped<ProductRepo>();
builder.Services.AddScoped<CustomerRepo>();
builder.Services.AddScoped<OrderRepo>();
builder.Services.AddScoped<QuotationComparisonRepo>();

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<DistributorService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "GadgetHub-API/1.0");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
{
    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
});
builder.Services.AddHttpClient<OrderProcessingService>();
builder.Services.AddHttpClient<ProductService>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<DistributorService>();
builder.Services.AddScoped<OrderProcessingService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddScoped<AuthService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseRouting();
app.UseAuthorization();

// Dynamic JS config so pages can call embedded API or separate API
app.MapGet("/js/api-config.js", (IConfiguration config) =>
{
    var apiBase = config["ApiBaseUrl"] ?? "";
    var electroCom = config["DistributorUrls:ElectroCom"] ?? "https://localhost:7077";
    var techWorld = config["DistributorUrls:TechWorld"] ?? "https://localhost:7102";
    var gadgetCentral = config["DistributorUrls:GadgetCentral"] ?? "https://localhost:7007";
    var js = $$"""
window.GH = window.GH || {};
window.GH.apiBase = {{System.Text.Json.JsonSerializer.Serialize(apiBase)}};
window.GH.distributors = {
  ElectroCom: {{System.Text.Json.JsonSerializer.Serialize(electroCom)}},
  TechWorld: {{System.Text.Json.JsonSerializer.Serialize(techWorld)}},
  GadgetCentral: {{System.Text.Json.JsonSerializer.Serialize(gadgetCentral)}}
};
window.GH.api = function (path) {
  var base = (window.GH.apiBase || '').replace(/\/$/, '');
  var p = path.charAt(0) === '/' ? path : '/' + path;
  return base ? base + p : p;
};
""";
    return Results.Content(js, "application/javascript");
});

app.MapRazorPages();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var webContext = scope.ServiceProvider.GetRequiredService<WebDbContext>();
    var apiContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    try
    {
        Console.WriteLine("Testing database connection...");
        var canConnect = await webContext.Database.CanConnectAsync();
        Console.WriteLine($"Database connection: {(canConnect ? "Success" : "Failed")}");

        if (canConnect)
        {
            await webContext.Database.EnsureCreatedAsync();
            await apiContext.Database.EnsureCreatedAsync();
            Console.WriteLine("Database ready");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database error: {ex.Message}");
    }
}

app.Run();
