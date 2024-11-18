using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"../Keys"))
    .SetApplicationName("SharedCookieApp");

builder.Services.AddAuthentication("Identity.Application")
    .AddCookie("Identity.Application", options =>
    {
        options.Cookie.Name = ".Identity.SharedCookie";
    });

builder.Services.AddAuthorization();

var clientOrigins = builder.Configuration.GetSection("ClientOrigins").Get<string>();
ArgumentNullException.ThrowIfNull(clientOrigins);

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowClientApp",
        policy => policy.WithOrigins(
                clientOrigins.Split(','))
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "API Service");

app.MapGet("/api/protected", async (HttpContext httpContext) =>
{
    await httpContext.Response.WriteAsJsonAsync(new { Message = "Accessed Protected" });
}).RequireAuthorization();

app.MapPost("/api/protected", async (HttpContext httpContext) =>
{
    await httpContext.Response.WriteAsJsonAsync(new { Message = "Posted to Protected" });
}).RequireAuthorization();

await app.RunAsync();