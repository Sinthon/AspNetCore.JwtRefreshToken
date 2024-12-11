using Api;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<JwtProvider>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AuthContext>(options =>
{
    options.UseSqlServer(builder.Configuration
        .GetConnectionString("Database"), 
            b => b.EnableRetryOnFailure(3));
});

builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddFastEndpoints();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthContext>();
    dbContext.Init();
}

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseFastEndpoints();

app.Run();
