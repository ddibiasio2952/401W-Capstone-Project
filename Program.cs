using FalveyInsuranceGroup.Backend.Services;
using FalveyInsuranceGroup.Db;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container. AddNewtonsoftJson is for the Patch method.
builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling =
            Newtonsoft.Json.ReferenceLoopHandling.Ignore
    );

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
});

// Add Swagger/Swashbuckle and inject certain services for certain controllers
// Add Swagger/Swashbuckle and inject certain services for certain controllers
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<InputService>();
builder.Services.AddScoped<MapService>();
builder.Services.AddScoped<HelperService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
options.AddPolicy("AllowFrontend",
    policy => {

        policy.WithOrigins("https://localhost:7288", "http://127.0.0.1:5500") // frontend dev server
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });

});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection failed.");
}

builder.Services.AddDbContext<FalveyInsuranceGroupContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.UseNetTopologySuite()
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FalveyInsuranceGroup API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();  // serves files from wwwroot
app.UseRouting();
app.UseCors("AllowFrontend"); // Enable CORS before MapControllers
app.UseSession();
app.UseAuthorization();
app.MapControllers();
app.Run();
