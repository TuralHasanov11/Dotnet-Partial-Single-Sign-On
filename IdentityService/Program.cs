using IdentityService.Features.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllersWithViews();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"../Keys"))
    .SetApplicationName("SharedCookieApp");

builder.Services.AddAuthentication("Identity.Application")
    .AddCookie("Identity.Application", options =>
    {
        options.Cookie.Name = ".Identity.SharedCookie";
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            },

            OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
        };
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

//app.UseRouting();

app.UseAuthorization();

//app.MapStaticAssets();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}")
//    .WithStaticAssets();


app.MapGet("/", () => "Identity Service");

app.MapPost("/api/identity/login", () =>
{
    return Results.SignIn(
        new ClaimsPrincipal(
            new ClaimsIdentity([
                new Claim(ClaimTypes.Name, "miguel"),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "miguel@test.com"),
                new Claim(ClaimTypes.Role, "Admin")
            ],
            "Identity.Application")),
            new AuthenticationProperties { IsPersistent = true });
});

app.MapPost("/api/identity/register", () =>
{
    return Results.SignIn(
        new ClaimsPrincipal(
            new ClaimsIdentity([
                new Claim(ClaimTypes.Name, "miguel"),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "miguel@test.com"),
                new Claim(ClaimTypes.Role, "Admin")
            ],
            "Identity.Application")),
            new AuthenticationProperties { IsPersistent = true });
});

app.MapPost("/api/identity/logout", () =>
{
    return Results.SignOut(authenticationSchemes: ["Identity.Application"]);
});

app.MapGet("/api/identity/user-info", (ClaimsPrincipal principal) =>
{
    var userInfo = new UserInfoResponse(
        principal.FindFirstValue(ClaimTypes.NameIdentifier)!,
        principal.FindFirstValue(ClaimTypes.Name)!,
        principal.FindFirstValue(ClaimTypes.Email)!,
        principal.FindAll(c => c.ValueType == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray());

    return TypedResults.Ok(userInfo);
}).RequireAuthorization();

app.MapGet("/api/protected", () =>
{
    return "Secret";
}).RequireAuthorization();

await app.RunAsync();