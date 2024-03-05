namespace WebDavToPaperlessNGX.Options;

public class WebDavToPaperlessOptions
{
    public const string SectionName = "WebDavToPaperless";
    
    public string? PaperlessUrl { get; set; }
    public string? PaperlessUser { get; set; }
    public string? PaperlessPassword { get; set; }
}