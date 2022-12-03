using MyORM;
using SharedLibrary.Models;
using System.Net;
using Scriban;

namespace GameServer.Controllers;


[HttpController("accounts")]
public class AccountsController: Controller
{
    [HttpGET]
    public List<Account> GetAccounts()
    {
        if(!HttpContext.Request.Cookies.CheckSessionId())
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        var dao =
            new AccountDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
        return dao.Select();
    }
    
    [HttpGET("{id:int}")]
    public Account GetAccountById(int id)
    {
        var dao =
            new AccountDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
        return dao.Select(id);
    }

    [HttpGET("info")]
    public Account GetAccountInfo()
    {
        int? id = HttpContext.Request.Cookies.CheckAuthorizedAccount();
        if (!id.HasValue)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return null;
        }
        var dao =
            new AccountDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
        return dao.Select(id.Value);
    }

    [HttpGET("auth/login")]
    public Html Login()
    {
        var account = MethodHandling.TryGetAccount(HttpContext);
        
        var data = File.ReadAllText(@"static\login.html");
        var template = Template.Parse(data);
        var htmlPage = template.Render(new{account = account});
        return new Html() { Page = htmlPage };
    }
    
    [HttpGET("auth/signup")]
    public Html Signup()
    {
        var account = MethodHandling.TryGetAccount(HttpContext);
        
        var data = File.ReadAllText(@"static\signup.html");
        var template = Template.Parse(data);
        var htmlPage = template.Render(new{account = account});
        return new Html() { Page = htmlPage };
    }
    
    [HttpGET("profile")]
    public Html GetProfile()
    {
        if(!HttpContext.Request.Cookies.CheckSessionId())
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return new Html(){Page = "<!DOCTYPE html><html><head><title>Ne zashel ti mujichok</title></head><body>not authorized</body>"};
        }
        var account = MethodHandling.TryGetAccount(HttpContext);
        
        var data = File.ReadAllText(@"static\account.html");
        var template = Template.Parse(data);
        var htmlPage = template.Render(new{account = account});
        return new Html() { Page = htmlPage };
    }

    [HttpPOST("profile")]
    public void UpdateProfile(string body)
    {
        var parsedBody = body.ParseAsQuery();
        var account = MethodHandling.TryGetAccount(HttpContext);
        var type = account.GetType();
        var dao = new AccountDao(
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
        foreach (var field in type.GetFields())
        {
            var abobus = field.GetValue(account);
            if (parsedBody.ContainsKey(field.Name) && !string.Equals(parsedBody[field.Name], field.GetValue(account).ToString(), StringComparison.InvariantCulture))
            {
                dao.Update(account.Id, string.Concat(field.Name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())) , parsedBody[field.Name]);
            }
        }

        HttpContext.Response.Redirect("/accounts/profile/");
    }
    
    [HttpPOST("auth/login")]
    public void Login(string body, string login, string password, string remember)
    {
        if (body == login || body == password || body == remember)
        {
            string[] parsedBody = body.ParseAsQueryToArray();
            if (parsedBody.Length == 3)
                (login, password, remember) = (parsedBody[0], parsedBody[1], parsedBody[2]);
            else if (parsedBody.Length == 2)
            {
                (login, password) = (parsedBody[0], parsedBody[1]);
            }
        }

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            return;

        Console.WriteLine(body + "  " + login + "  " + password);

        Console.WriteLine($"Check this method: login: {login} password: {password}");
        var dao = new AccountDao(
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
        var account = dao.Select().FirstOrDefault(acc => acc.Name == login && acc.Password == password);
        if (account != null)
        {
            var guid = Guid.NewGuid();
            var session = new Session(guid, account.Id, account.Email, DateTime.Now);

            SessionManager.CreateOrGetSession(guid, () => session);
            SessionManager.CheckSession(guid);
            
            HttpContext.Response.AppendHeader("Set-Cookie", $"SessionId={session.Id}; path=/");
        }

        HttpContext.Response.Redirect("/accounts/profile");
    }

    [HttpPOST("auth/signup")]
    public void SaveAccount(string login, string email, string phone, string password)
    {
        var dao =
            new AccountDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
        dao.Insert(new Account(login, email, phone, password));

        var account = dao.Select().FirstOrDefault(acc => acc.Name == login && acc.Password == password);
        var guid = Guid.NewGuid();
        var session = new Session(guid, account.Id, account.Email, DateTime.Now);

        SessionManager.CreateOrGetSession(guid, () => session);
        SessionManager.CheckSession(guid);

        HttpContext.Response.AppendHeader("Set-Cookie", $"SessionId={session.Id}; path=/");

        HttpContext.Response.Redirect("/accounts/profile");
    }

    [HttpDELETE("delete")]
    public string DeleteAllAccounts()
    {
        var dao =
            new AccountDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
        dao.Delete();

        return "delete all accounts";
    }
    
    [HttpDELETE("delete/{id:int}")]
    public string DeleteAccount(int id)
    {
        var dao =
            new AccountDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
        dao.Delete(id);

        return $"delete account by id = {id}";
    }

    [HttpGET("quit")]
    public void QuitAccount()
    {
        if (HttpContext.Request.Cookies["SessionId"] != null)
        {
            var myCookie = HttpContext.Request.Cookies["SessionId"];
            myCookie.Path = "/";
            myCookie.Expires = DateTime.Now.AddDays(-1d);
            HttpContext.Response.Cookies.Add(myCookie);
        }
        HttpContext.Response.Redirect("/");
    }

    #region Test methods
    //http://localhost:1337/accounts/login?email=aboba&password=123
    //http://localhost:1337/accounts/login?email=aboba&password=prvi
    [HttpGET("login")]
    public bool GetLogin(string email, string password)
    {
        Console.WriteLine($"Check this method: email: {email} password: {password}");
        var dao = new AccountDao(
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
        return dao.Select().Any(acc => acc.Email == email && acc.Password == password);
    }

    [HttpGET("update")]
    public string UpdateAccountInfo(int id, string tableName, string newValue)
    {
        var dao = new AccountDao(
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
        dao.Update(id, tableName, newValue);
        return $"update {tableName} with this value - {newValue}";
    }


    //http://localhost:1337/accounts/createAccount?name=aboba&email=aobba@mail.ru&phone=1337228&password=
    [HttpGET("createAccount")]
    public string CreateAccount(string name, string email, string phone, string password)
    {
        var dao = new AccountDao(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameServerDB;Integrated Security=True;");
        dao.Insert(new Account() { Name = name, Email = email, Phone = phone, Password = password });
        return $"{name}, {email}, {phone}, {password}";
    }
    #endregion
}