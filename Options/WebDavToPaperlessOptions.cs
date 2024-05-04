namespace WebDavToPaperlessNGX.Options;

public class WebDavToPaperlessOptions
{
    public const string SectionName = "WebDavToPaperless";

    public string? PaperlessBaseUrl { get; set; }
    public string? PaperlessUrlSegment { get; set; }
    public string? PaperlessUser { get; set; }
    public string? PaperlessPassword { get; set; }

}
