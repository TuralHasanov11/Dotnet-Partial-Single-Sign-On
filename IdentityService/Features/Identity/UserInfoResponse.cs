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

    public string Id { get; init; }

    public string Name { get; init; }

    public string Email { get; init; }
}