using System.Data;
using System.Data.SqlClient;

namespace YaSkamerBroServer;


[HttpController("accounts")]
public class Accounts
{
    [HttpGET("list")]
    public List<Account> GetAccounts()
    {
        List<Account> accounts = new List<Account>();

        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ServerDB;Integrated Security=True;";

        string sqlExpression = "SELECT * FROM Accounts";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand command = new SqlCommand(sqlExpression, connection);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", reader.GetName(0), reader.GetName(1), reader.GetName(2), reader.GetName(3), reader.GetName(4));

                while (reader.Read())
                {
                    accounts.Add(new Account() 
                    { 
                        Id = reader.GetInt32(0), 
                        Name = reader.GetString(1), 
                        Email = reader.GetString(2), 
                        Phone = reader.GetString(3), 
                        Password = reader.GetString(4) 
                    });
                    /*int id = reader.GetInt32(0);
                    string name = reader.GetString(1);

                    Console.WriteLine("{0} \t{1}", id, name);*/
                }
            }

            reader.Close();
        }

        return accounts;
    }

    [HttpGET("getById")]
    public Account GetAccountById(int id)
    {
        return GetAccounts()[id];
    }

    public Account SaveAccount()
    {
        throw new NotImplementedException();
    }
    //GetAccounts (/accounts/) - список акков в формате жсон
    //Get /accounts/{id} - возвращает акк по индексу
    //Post /accounts/ - добавляет инфу на сервер
}

public class Account
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
    public string Phone { get; init; }
    public string Password { get; init; }
}