namespace WebDavToPaperlessNGX.Middlewares;

public static class PaperlessHelper
{
    public static MultipartFormDataContent CreatePaperlessFormDataContent(Stream document, PaperlessDocumentOptionalData paperlessDocumentOptionalData)
    {
        var res = new MultipartFormDataContent();
        res.Add(new StreamContent(document), "document", "document.pdf");
        if (paperlessDocumentOptionalData.Title != null) { res.Add(new StringContent(paperlessDocumentOptionalData.Title), "title");}
        if (paperlessDocumentOptionalData.Created != null) { res.Add(new StringContent(paperlessDocumentOptionalData.Created.Value.ToString("yyyy-MM-dd")), "created");}
        if (paperlessDocumentOptionalData.Correspondent != null) { res.Add(new StringContent(paperlessDocumentOptionalData.Correspondent), "correspondent");}
        if (paperlessDocumentOptionalData.DocumentType != null) { res.Add(new StringContent(paperlessDocumentOptionalData.DocumentType), "document_type");}
        if (paperlessDocumentOptionalData.StoragePath != null) { res.Add(new StringContent(paperlessDocumentOptionalData.StoragePath), "storage_path");}
        if (paperlessDocumentOptionalData.ArchiveSerialNumber != null) { res.Add(new StringContent(paperlessDocumentOptionalData.ArchiveSerialNumber), "archive_serial_number");}
  
        if (paperlessDocumentOptionalData.Tags != null)
        {
            foreach (var tag in paperlessDocumentOptionalData.Tags)
            {
                res.Add(new StringContent(tag), "tags");
            }
        }
  
        return res;
    }
}