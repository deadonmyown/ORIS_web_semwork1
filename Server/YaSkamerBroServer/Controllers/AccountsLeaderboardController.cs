using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SharedLibrary.Models;
using MyORM;
using Scriban;

namespace GameServer.Controllers
{
    [HttpController("leaderboard")]
    public class AccountsLeaderboardController : Controller
    {
        [HttpGET("list")]
        public List<AccountsLeaderboard> GetResults()
        {
            var dao =
                new AccountsLeaderboardDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
            return dao.Select();
        }
        
        [HttpGET]
        public Html GetLeaderboard()
        {
            var account = MethodHandling.TryGetAccount(HttpContext);
            var leaderboard = GetResults();
            var data = File.ReadAllText(@"static\leaderboard.html");
            var template = Template.Parse(data);
            var htmlPage = template.Render(new{account = account, leaderboard = leaderboard});
            return new Html() { Page = htmlPage };
        }

        [HttpPOST]
        public string PostNewResult(string body)
        {
            Console.WriteLine(body);
            return body;
        }

        [HttpPOST("update")]
        public void UpdateLeaderboard()
        {
            var dao =
               new AccountsLeaderboardDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
            dao.InsertFromAccountsTime();
        }
    }
}
