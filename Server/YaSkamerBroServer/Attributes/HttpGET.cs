namespace YaSkamerBroServer;

public class HttpGET: Attribute
{
    public string MethodURI;

    public HttpGET(string methodURI)
    {
        MethodURI = methodURI;
    }
}