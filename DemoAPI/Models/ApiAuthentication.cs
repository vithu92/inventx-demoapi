using System;

namespace DemoAPI.Models;

public class ApiAuthentication
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string AESKey { get; set; } = string.Empty;
    public string AESIV { get; set; } = string.Empty;
}
