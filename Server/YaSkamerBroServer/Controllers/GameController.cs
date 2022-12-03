using System.Net;
using Scriban;

namespace GameServer.Controllers;

[HttpController("")]
public class GameController : Controller
{
    [HttpGET]
    public Html GetIndexHtml()
    {
        var account = MethodHandling.TryGetAccount(HttpContext);

        var data = File.ReadAllText(@"static\index.html");
        var template = Template.Parse(data);
        var htmlPage = template.Render(new { account = account });
        return new Html() { Page = htmlPage };
    }
}