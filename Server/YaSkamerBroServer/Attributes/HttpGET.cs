namespace GameServer;

public class HttpGET: HttpMethodAttribute
{
    public HttpGET(string methodUri) : base(HttpMethod.Get, methodUri)
    {
    }
    
    public HttpGET() : this("") {}
}