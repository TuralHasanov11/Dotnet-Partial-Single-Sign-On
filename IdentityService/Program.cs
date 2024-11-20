using IdentityService.Features.Identity;
using IdentityService.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllersWithViews();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"../Keys"))
    .SetApplicationName("SharedCookieApp");

var gitHubSettings = builder.Configuration
    .GetSection(GitHubSettings.SectionName)
    .Get<GitHubSettings>();

ArgumentNullException.ThrowIfNull(gitHubSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        //options.Cookie.Name = ".Identity.SharedCookie";
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
    })

    //.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    //{
    //    options.Authority = gitHubSettings.Authority;
    //    options.ClientId = gitHubSettings.ClientId;
    //    options.ClientSecret = gitHubSettings.ClientSecret;

    //    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    //    options.ResponseType = OpenIdConnectResponseType.Code;

    //    options.SaveTokens = true;
    //    options.UsePkce = true;
    //    options.GetClaimsFromUserInfoEndpoint = true;

    //    options.CallbackPath = new PathString("/signin-oidc");

    //    options.Configuration = new OpenIdConnectConfiguration
    //    {
    //        AuthorizationEndpoint = "https://github.com/login/oauth/authorize",
    //        TokenEndpoint = "https://github.com/login/oauth/access_token",
    //        UserInfoEndpoint = "https://api.github.com/user"
    //    };

    //    //options.MapInboundClaims = false;
    //    //options.TokenValidationParameters.NameClaimType = ClaimTypes.Name;
    //    //options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
    //});
    .AddOAuth(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = gitHubSettings.ClientId;
        options.ClientSecret = gitHubSettings.ClientSecret;
        options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
        options.TokenEndpoint = "https://github.com/login/oauth/access_token";
        options.UserInformationEndpoint = "https://api.github.com/user";
        options.CallbackPath = new PathString("/signin-oidc");
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Events.OnCreatingTicket = async (context) =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
            using var result = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            result.EnsureSuccessStatusCode();
            var user = await result.Content.ReadFromJsonAsync<JsonElement>();
            context.RunClaimActions(user);
        };

        options.SaveTokens = false;

        options.ClaimActions.MapJsonKey("sub", "id");
        options.ClaimActions.MapJsonKey("name", "login");
        options.ClaimActions.MapJsonKey("email", "email");
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
            CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = true });
});

app.MapGet("/api/identity/login/github", (string? returnUrl = "/") =>
{
    var githubAuthenticationProperties = new AuthenticationProperties
    {
        RedirectUri = returnUrl
    };
    return Results.Challenge(
        githubAuthenticationProperties,
        authenticationSchemes: [
            OpenIdConnectDefaults.AuthenticationScheme
        ]);
});

app.MapPost("/api/identity/register", () =>
{
    return Results.SignIn(
        new ClaimsPrincipal(
            new ClaimsIdentity([
                new Claim("sub", "miguel"),
                new Claim("name", Guid.NewGuid().ToString()),
                new Claim("email", "miguel@test.com"),
            ],
            "Identity.Application")),
            new AuthenticationProperties { IsPersistent = true });
});

app.MapPost("/api/identity/logout", () =>
{
    return Results.SignOut(authenticationSchemes: [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
});

app.MapGet("/api/identity/user-info", (ClaimsPrincipal principal) =>
{
    var userInfo = new UserInfoResponse(
        principal.FindFirstValue("sub")!,
        principal.FindFirstValue("name")!,
        principal.FindFirstValue("email")!,
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