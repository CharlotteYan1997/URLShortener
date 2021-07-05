using System;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using LiteDB;
using System.Linq;

/// <summary>
/// Initialize a host object which encapsulates the application resources such as Dependency Injection, Logging, and Configuration.
/// In the initialization of the host object, we use attribute routing to model our application for REST APIs.
/// </summary>
var host = Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
{
    // Configure the application's services
    webBuilder.ConfigureServices(services =>
    {
        // Add services required for routing requests.
        services.AddRouting();

        // Add LiteDB as a storage for the customized URLs generated. 
        services.AddSingleton<ILiteDatabase, LiteDatabase>(_ => new LiteDatabase("customURL.db"));
    })
    .Configure(application =>
    {
        // Add route matching to the middleware pipeline.
        // This middleware looks at the set of endpoints defined in the application, and selects the best match based on the request.
        application.UseRouting();

        // Add endpoint execution to the middleware pipeline.
        // It runs the delegate associated with the selected endpoint.
        application.UseEndpoints((endpoints) =>
            {
                // Define the route index.html to be an endpoint
                endpoints.MapGet("/", context =>
                {
                    return context.Response.SendFileAsync("index.html");
                });

                // The /shortLink route gets the POST response from the UI
                endpoints.MapPost("/shortLink", GetShortLink);
                endpoints.MapFallback(RedirectURL);
            });
    });
})
.Build();

await host.RunAsync();

#region API Endpoints
/// <summary>
/// Create a short link from the original url entered by the user.
/// </summary>
static Task GetShortLink(HttpContext context)
{
    // Validate the input context to ensure we are getting data from HTML form, and the form
    // contains the url data in the request. 
    // Write the error in the browser if we have invalid input context.
    if (!context.Request.HasFormContentType || !context.Request.Form.ContainsKey("originalUrl"))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return context.Response.WriteAsync("Cannot process request");
    }

    // Get the originalUrl String from user's input
    context.Request.Form.TryGetValue("originalUrl", out var originalUrlFormData);
    var originalURL = originalUrlFormData.ToString();

    // Get the custom segment String from user's input
    context.Request.Form.TryGetValue("customUrl", out var customUrlFormData);
    var customSegment = customUrlFormData.ToString();

    // Create a new Uri using the originalURL.
    // If the Uri cannot be successfully created, return BadRequest.
    if (!Uri.TryCreate(originalURL, UriKind.Absolute, out Uri uri))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return context.Response.WriteAsync("Invalid URL");
    }

    // Get the collection of customURLs and insert the newly generated customURL from the uri
    var links = context.RequestServices.GetService<ILiteDatabase>().GetCollection<CustomURL>(BsonAutoId.Int32);
    var customURL = new CustomURL
    {
        Url = uri.ToString()
    };
    links.Insert(customURL);

    // Get the shortLink and create a new uri to be redirected
    var shortLink = customURL.ShortenUrl();
    var shortLinkUri = customSegment == "" ? $"{context.Request.Scheme}://{context.Request.Host}/{shortLink}" : $"{context.Request.Scheme}://{context.Request.Host}/{customSegment}/{shortLink}";
    context.Response.Redirect($"/#{shortLinkUri}");

    return Task.CompletedTask;
}

/// <summary>
/// Set the redirect URL with the short link.
/// </summary>
static Task RedirectURL(HttpContext context)
{
    var customURLs = context.RequestServices.GetService<ILiteDatabase>().GetCollection<CustomURL>();
    string[] pathComponents = context.Request.Path.ToUriComponent().Split('/');
    string path = pathComponents[pathComponents.Length - 1];
    var id = CustomURL.GetId(path);
    var customURL = customURLs.Find(p => p.Id == id).FirstOrDefault();

    if (customURL != null)
    {
        context.Response.Redirect(customURL.Url);
    }
    else
    {
        context.Response.Redirect("/");
    }

    return Task.CompletedTask;
}

#endregion API Endpoints