# SecurityInnovation Coding Challenge - Custom URL Shortener

Short links look much better when we need to share any long URLs 
with our friends. This custom URL Shortener tool provides a service 
to turn these long, messy URLs into shorter and cleaner URLs.

## Built With
- C#
- LiteDB
- JavaScript
- HTML

## Design & Solution
The goal of this project is to build a URL shortener web application that simply takes a URL and a custom keyword from the user's input to generate a short link.
By clicking on the short link with the customized keyword, the user will be redirected to the original link that the user inputs. Considering the requirements, we need

- An interactive user interface to take user's input
- Backend code that handles the encoding and decoding of the URLs with a webserver and API Endpoints
- A database that stores our data

### Backend
The `CustomURL.cs` is a class for the `CustomURL` object which contains two properties
- `Id`: Index in the db to find a url
- `Url`: Represents the original url

It contains two methods that handle the encoding and decoding of URLs.
- `ShortenUrl`: Encodes the Id property into a short String by using `Base64UrlEncode`.
- `GetId`: Decodes the short link into an interger Id by using `Base64UrlDecode`.

The endpoint that receives a POST request to the `/shortLink` route is added in the `program.cs` file, which takes the user's input.
The `program.cs` file first initialize a host object which encapsulates the application resources such as Dependency Injection, Logging, and Configuration. In the initialization of the host object, we use attribute routing to model our application for REST APIs.
It contains two API Endpoints `GetShortLink` and `RedirectURL`.
- `GetShortLink`: Creates a short link from the original url entered by the user. 
If the user has specified a keyword, append the keyword to the shortened URL. It first validates the input HTTP context, get the shortLink by calling the `ShortenUrl()` method from the `CustomURL` obejct, and create a new uri to be redirected.
- `RedirectURL`: Sets the redirect URL with the short link. We parse the URL back into an integer id, find that id in our LiteDB collection, and then redirect to that URL if exists.

### Database
In this project, I used LiteDB to store data. LiteDB is a simple, lightweight NoSQL database. It is a serverless database delivered in a single small DLL fully written in .NET C# managed code.
LiteDB allows us to easily persist our short links to a file (`customURL.db`) so they can be retrived later. 

LiteDB is added to our available services in the Program.cs file. From there, we use the LiteDB API to get a CustomURL collection and insert it to our database.

### Frontend
The frontend contains a simple form that takes a url and a keyword from the user's input. If the backend returns any errors, those errors will be shown in the browser.

## Installation

If you have Visual Studio, you can open the solution by clicking on the `UrlShortener.csproj` file. You can also run the project with the `dotnet` command.

```bash
dotnet run
```