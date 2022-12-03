namespace GameServer;

public class HttpController: Attribute
{
    public string ControllerName;

    public HttpController(string controllerName)
    {
        ControllerName = controllerName;
    }

    public HttpController() : this(null) {}
}