namespace GameServer;

public class HttpMethodAttribute : Attribute
{
    public HttpMethod HttpMethod { get; set; }
    
    public string MethodUri { get; set; }

    public HttpMethodAttribute(HttpMethod httpMethod, string methodUri)
    {
        HttpMethod = httpMethod;
        MethodUri = methodUri;
    }
}