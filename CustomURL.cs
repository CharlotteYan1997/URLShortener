using System;
using Microsoft.AspNetCore.WebUtilities;

/// <summary>
/// The CustomURL class
/// Contains all methods for converting a URL to a short link
/// </summary>
#pragma warning disable CA1050 // Declare types in namespaces
public class CustomURL
#pragma warning restore CA1050 // Declare types in namespaces
{
    /// <summary>
    /// Encode the 'Id' field using base64url encoding
    /// </summary>
    public string ShortenUrl()
    {
        return WebEncoders.Base64UrlEncode(BitConverter.GetBytes(Id));
    }

    /// <summary>
    /// Decode the encoded url to an Integer representation
    /// </summary>
    public static int GetId(string url)
    {
        return BitConverter.ToInt32(WebEncoders.Base64UrlDecode(url));
    }

    // The 'Id' property is an index in the db to find a url
    public int Id { get; set; }

    // The 'Url' property represents the original url
    public string Url { get; set; }
}