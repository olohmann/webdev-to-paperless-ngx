using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Serilog;
using WebDavToPaperlessNGX.Options;

namespace WebDavToPaperlessNGX.Middlewares;

public class WebDavToPaperlessMiddleware(RequestDelegate? next/*, IOptions<WebDavToPaperlessOptions> options*/)
{

    public async Task InvokeAsync(HttpContext httpContext, IOptions<WebDavToPaperlessOptions> options)
    {
        var request = httpContext.Request;
        var response = httpContext.Response;
        
        // TODO: Segment verification is not optimal. It should be done in a more robust way.
        if (!request.Path.Value!.Contains($"/{options.Value.PaperlessUrlSegment}"))
        {
            Log.Information("Request is NOT for WebDavToPaperless. Calling next middleware.");
            
            if (next != null)
            {
                await next(httpContext);
            }

            return;
        }

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
                    await HandleDocumentPut(response, request, options);
                    break;
                }
            case "LOCK":
                {
                    response.Headers.AcceptRanges = "bytes";
                    response.Headers.ContentLength = 0;
                    response.StatusCode = 404;
                    break;
                }
            case "UNLOCK":
                {
                    response.Headers.AcceptRanges = "bytes";
                    response.Headers.ContentLength = 0;
                    response.StatusCode = 200;
                    break;
                }
            default:
                {
                    response.Headers.AcceptRanges = "bytes";
                    response.Headers.ContentLength = 0;
                    response.StatusCode = 200;
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

    private static async Task HandleDocumentPut(HttpResponse httpResponse, HttpRequest httpRequest, IOptions<WebDavToPaperlessOptions> options)
    {
        httpResponse.Headers.AcceptRanges = "bytes";
        httpResponse.Headers.ContentLength = 0;

        // "Pseudo" File Creation
        if (httpRequest.Headers.ContentLength == 0)
        {
            httpResponse.StatusCode = 201;
            return;
        }

        // Actual Content Upload
        try
        {
            var optionalData = new PaperlessDocumentOptionalData();
            

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(options.Value.PaperlessBaseUrl ?? throw new InvalidOperationException("Paperless URL is not set."));
            
            var authenticationString = $"{options.Value.PaperlessUser}:{options.Value.PaperlessPassword}";
            
            var base64EncodedAuthenticationString =
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));
            
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/documents/post_document/");
            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            
            var multipartFormDataContent =
                PaperlessHelper.CreatePaperlessFormDataContent(httpRequest.Body, optionalData);
            requestMessage.Content = multipartFormDataContent;
            var responseMessage = await httpClient.SendAsync(requestMessage);

            if (!responseMessage.IsSuccessStatusCode)
            {
                Log.Logger.Error($"Paperless API: Failed to upload document to Paperless. Request: {requestMessage.RequestUri}. Response Status Code: {(int)responseMessage.StatusCode}. Reason Phrase: {responseMessage.ReasonPhrase}");
                httpResponse.StatusCode = 500;
                return;
            }

            Log.Logger.Information("Paperless API: Successfully uploaded document to Paperless.");
            httpResponse.StatusCode = 200;
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Paperless API: Failed to upload document to Paperless.");
            httpResponse.StatusCode = 500;
        }
    }
}
