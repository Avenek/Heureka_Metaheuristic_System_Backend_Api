using FluentValidation.AspNetCore;
using Heureka_Metaheuristic_System_Backend_Api.Database;
using Heureka_Metaheuristic_System_Backend_Api.Middleware;
using Heureka_Metaheuristic_System_Backend_Api.Registrars;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using NLog.Web;
using DotNetEnv;

var path = Path.GetFullPath("../../.env");

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName.ToLower();

Env.Load(path);
Env.Load($"{path}.{environment}");

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
}, ServiceLifetime.Transient);

Registar registar = new Registar();
registar.ConfigureServices(builder.Services);


builder.Host.UseNLog();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontEndClient", b =>
        b.AllowAnyMethod()
            .AllowAnyHeader()
             .SetIsOriginAllowed(origin => true)
    );
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

var app = builder.Build();

app.UseCors("FrontEndClient");


app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
    });
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.UseEndpoints(endpoints => endpoints.MapControllers());

app.Run();
