using IdentityService.Features.Identity;
using IdentityService.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

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
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
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
    })
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

        options.Scope.Add("read:user");
        options.Scope.Add("user:email");

        options.ClaimActions.MapJsonKey("id", "id");
        options.ClaimActions.MapJsonKey(JwtRegisteredClaimNames.Name, "login");
        options.ClaimActions.MapJsonKey(JwtRegisteredClaimNames.Email, "email");
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
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/", () => "Identity Service");

app.MapPost("/api/identity/login", () =>
{
    return Results.SignIn(
        new ClaimsPrincipal(
            new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Name, "Dexter"),
                new Claim("id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, "dexter@test.com"),
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

app.MapPost("/api/identity/logout", () =>
{
    return Results.SignOut(
        authenticationSchemes: [
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme]);
});

app.MapGet("/api/identity/user-info", (ClaimsPrincipal principal) =>
{
    var userInfo = new UserInfoResponse(
        principal.FindFirstValue("id")!,
        principal.FindFirstValue(JwtRegisteredClaimNames.Name)!,
        principal.FindFirstValue(JwtRegisteredClaimNames.Email)!);

    return TypedResults.Ok(userInfo);
}).RequireAuthorization();

app.MapGet("/api/protected", () =>
{
    return "Secret";
}).RequireAuthorization();

await app.RunAsync();