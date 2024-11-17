namespace IdentityService.Features.Identity;

public record UserInfoResponse
{
    public UserInfoResponse(
        string id,
        string name,
        string email)
    {
        Id = id;
        Name = name;
        Email = email;
    }

    public UserInfoResponse(
        string id,
        string name,
        string email,
        IEnumerable<string>? roles)
    {
        Id = id;
        Name = name;
        Email = email;
        Roles = roles;
    }

    public string Id { get; init; }

    public string Name { get; init; }

    public string Email { get; init; }

    public IEnumerable<string>? Roles { get; init; }
}