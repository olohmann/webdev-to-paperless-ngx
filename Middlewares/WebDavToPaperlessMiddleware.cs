using System.Net.Http.Headers;
using Serilog;

namespace WebDavToPaperlessNGX.Middlewares;

public class WebDavToPaperlessMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var request = httpContext.Request;
        var response = httpContext.Response;
        
        switch (request.Method)
        {
            case "PROPFIND" or "GET":
            {
                await HandleGetPropfind(response, request);
                break;
            }
            case "HEAD":
            {
                await HandleHead(response, request);
                break;
            }
            case "PUT":
            {
                await HandleDocumentPut(response, request);
                break;
            }
            default:
            {
                await HandleOthers(response, request);
                break;
            }
        }
    }

    private const string WebdavEmptyCollectionResponse =
        @"<?xml version=""1.0"" encoding=""UTF-8""?>
                         <D:multistatus xmlns:D=""DAV:"">
                           <D:response>
                             <D:href>/</D:href>
                             <D:propstat>
                               <D:prop>
                                 <D:displayname>/</D:displayname>
                                 <D:resourcetype><D:collection/></D:resourcetype>
                                 <D:getlastmodified>Wed, 01 Jan 2020 00:00:00 GMT</D:getlastmodified>
                                 <D:creationdate>2020-01-01T00:00:00Z</D:creationdate>
                               </D:prop>
                               <D:status>HTTP/1.1 200 OK</D:status>
                             </D:propstat>
                           </D:response>
                         </D:multistatus>";

    private static async Task HandleGetPropfind(HttpResponse response, HttpRequest request)
    {
        response.ContentType = "application/xml";
        var xmlResponse = WebdavEmptyCollectionResponse;
        response.StatusCode = 200;

        await response.WriteAsync(xmlResponse);
    }

    private static Task HandleHead(HttpResponse response, HttpRequest request)
    {
        response.Headers.AcceptRanges = "bytes";
        response.Headers.ContentLength = 0;
        response.StatusCode = 404;
        return Task.CompletedTask;
    }

    private static Task HandleOthers(HttpResponse response, HttpRequest request)
    {
        response.Headers.AcceptRanges = "bytes";
        response.Headers.ContentLength = 0;
        response.StatusCode = 200;
        return Task.CompletedTask;
    }

    private static async Task HandleDocumentPut(HttpResponse httpResponse, HttpRequest httpRequest)
    {
        httpResponse.Headers.AcceptRanges = "bytes";
        httpResponse.Headers.ContentLength = 0;
        var uploadsDirectory = "UploadedFiles";
        Directory.CreateDirectory(uploadsDirectory);

        // File Creation
        if (httpRequest.Headers.ContentLength == 0)
        {
            httpResponse.StatusCode = 201;
            return;
        }

        // Actual Content Upload
        try
        {
            var optionalData = new PaperlessDocumentOptionalData();
            var multipartFormDataContent =
                PaperlessHelper.CreatePaperlessFormDataContent(httpRequest.Body, optionalData);

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://paperless.internal.lohmann.io");
            var authenticationString = $"paperless:MDLNRXcdzDBEiQ";
            var base64EncodedAuthenticationString =
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/documents/post_document/");
            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            requestMessage.Content = multipartFormDataContent;
            var responseMessage = await httpClient.SendAsync(requestMessage);

            if (!responseMessage.IsSuccessStatusCode)
            {
                httpResponse.StatusCode = 500;
                Log.Logger.Information("Failed to upload document to Paperless.");
                return;
            }

            Log.Logger.Information("Successfully uploaded document to Paperless.");
            httpResponse.StatusCode = 200;
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Failed to upload document to Paperless.");
            httpResponse.StatusCode = 500;
        }
    }
}