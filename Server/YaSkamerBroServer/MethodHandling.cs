using System.Net;
using MyORM;
using SharedLibrary.Models;

namespace GameServer;

public static class MethodHandling
{
    public static Account TryGetAccount(HttpListenerContext httpContext)
    {
        int? id = httpContext.Request.Cookies.CheckAuthorizedAccount();
        if (!id.HasValue)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        var dao =
            new AccountDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
        return dao.Select(id.Value);
    }

    public static List<Updates> TryGetUpdates()
    {
        var dao =
            new UpdatesDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
        return dao.Select();
    }

    public static Updates TryGetUpdate(string theme)
    {
        var dao =
            new UpdatesDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
        return dao.Select().FirstOrDefault(upd => upd.Theme == theme);
    }

    public static UpdateContent TryGetUpdateContent(int id)
    {
        var dao =
            new UpdateContentDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
        var content = dao.Select().FirstOrDefault(cnt => cnt.UpdateId == id);
        if (content == null)
        {
            var newContent = new UpdateContent() { AccountId = string.Empty, Content = "no content", ContentDate = DateTime.Now.ToString(), UpdateId = id };
            return newContent;
        }
        return content;
    }
}