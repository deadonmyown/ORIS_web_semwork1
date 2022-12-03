using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyORM;
using SharedLibrary.Models;

namespace GameServer.Controllers
{
    [HttpController("updates")]
    public class UpdatesController : Controller
    {
        [HttpGET]
        public Html GetUpdate()
        {
            var account = MethodHandling.TryGetAccount(HttpContext);
            var updates = MethodHandling.TryGetUpdates();

            var data = File.ReadAllText(@"static\update.html");
            var template = Template.Parse(data);
            var htmlPage = template.Render(new { account = account, updates = updates});
            return new Html() { Page = htmlPage };
        }

        [HttpGET("add")]
        public Html GetAddMenu()
        {
            var account = MethodHandling.TryGetAccount(HttpContext);

            var data = File.ReadAllText(@"static\add.html");
            var template = Template.Parse(data);
            var htmlPage = template.Render(new { account = account });
            return new Html() { Page = htmlPage };
        }

        [HttpPOST("add")]
        public void PostAdd(string body)
        {
            var parsedBody = System.Web.HttpUtility.ParseQueryString(body);

            var currDate = DateTime.Now.ToString();
            var update = new Updates() { Theme = parsedBody["Theme"], AccountName = parsedBody["AccountName"], Date = currDate, AccountId = string.Empty };


            var dao =
                new UpdatesDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
            dao.Insert(update);

            var newUpdate = MethodHandling.TryGetUpdate(parsedBody["Theme"]);
            var content = MethodHandling.TryGetUpdateContent(newUpdate.Id);

            var daoContent =
                new UpdateContentDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
            daoContent.Insert(content);

            HttpContext.Response.Redirect("/updates/");
        }

        [HttpPOST("redirect")]
        public void GetUpdateContent(string body)
        {
            var parsedBody = System.Web.HttpUtility.ParseQueryString(body);
            var updates = MethodHandling.TryGetUpdate(parsedBody["Theme"]);

            HttpContext.Response.Redirect($"{updates.Id}");
        }

        [HttpGET("redirect/{id:int}")]
        public Html GetUpdateContentById(int id)
        {
            var account = MethodHandling.TryGetAccount(HttpContext);
            var content = MethodHandling.TryGetUpdateContent(id);

            var data = File.ReadAllText(@"static\redirect.html");
            var template = Template.Parse(data);
            var htmlPage = template.Render(new { account = account, updatecontent = content });
            return new Html() { Page = htmlPage };
        }

        [HttpGET("redirect/{id:int}/edit")]
        public Html GetEditContent(int id)
        {
            var account = MethodHandling.TryGetAccount(HttpContext);
            var content = MethodHandling.TryGetUpdateContent(id);

            var data = File.ReadAllText(@"static\edit.html");
            var template = Template.Parse(data);
            var htmlPage = template.Render(new { account = account, updatecontent = content });
            return new Html() { Page = htmlPage };
        }

        [HttpPOST("redirect/{id:int}/edit")]
        public void PostEditContent(int id, string body)
        {
            var parsedBody = System.Web.HttpUtility.ParseQueryString(body);
            var text = parsedBody["Content"];

            var dao = 
                new UpdateContentDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
            dao.UpdateInEditMode(id, text);

            HttpContext.Response.Redirect($"/updates/redirect/{id}");
        }
    }
}
