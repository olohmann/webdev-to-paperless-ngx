namespace WebDavToPaperlessNGX.Middlewares;

public class PaperlessDocumentOptionalData
{
    public string? Title { get; set; }
    public DateTime? Created { get; set; }
    public string? Correspondent { get; set; }
    public string? DocumentType { get; set; }
    public string? StoragePath { get; set; }
    public string[]? Tags { get; set; }
    public string? ArchiveSerialNumber { get; set; }
}