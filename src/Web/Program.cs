using CleanArchitecture.Infrastructure.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// builder.Services.AddKeyVaultIfConfigured(builder.Configuration);
// Optionally, add services related to application, infrastructure, etc.
builder.Services.AddApplicationServices();  // Assuming this includes services like sender, etc.
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices();


// Register endpoint services (for API routing)
builder.Services.AddEndpointsApiExplorer(); // Enable endpoint routing discovery
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Use Swagger in the pipeline
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    options.RoutePrefix = string.Empty; // Set Swagger UI at the root URL
});


// Other middlewares (e.g., authorization, health checks, etc.)
app.UseHttpsRedirection();
// app.UseAuthentication();
app.UseAuthorization(); // Add authorization if required
app.MapControllers(); // In case you have other controller-based routes

app.Run();
