namespace IdentityService.Options;

public class GitHubSettings
{
    public const string SectionName = "GitHub";

    public string Authority { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }
}