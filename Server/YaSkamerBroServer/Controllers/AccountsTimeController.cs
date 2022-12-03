using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedLibrary.Models;
using MyORM;
using System.Net;

namespace GameServer.Controllers
{
    [HttpController("times")]
    public class AccountsTimeController : Controller
    {
        [HttpGET]
        public List<AccountsTime> GetResults()
        {
            var dao =
                new AccountsTimeDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
            return dao.Select();
        }

        [HttpPOST]
        public string PostResult(string body)
        {
            if (!HttpContext.Request.Cookies.CheckSessionId())
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "bad request";
            }
            var account = MethodHandling.TryGetAccount(HttpContext);

            Console.WriteLine(body);
            AccountsTime accountTime = JsonConvert.DeserializeObject<AccountsTime>(body);
            accountTime.Id = account.Id;
            Console.WriteLine(accountTime.ToString() ?? "accountTime is null");
            if (accountTime != null)
            {
                var dao =
                    new AccountsTimeDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
                dao.Insert(accountTime);
            }
            return body;
        }
    }
}
