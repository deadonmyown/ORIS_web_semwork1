using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Reflection;
using SharedLibrary.Models;

namespace MyORM
{
    public class AccountsLeaderboardDao
    {
        private readonly MyOrm _myOrm;
        private readonly string _stringConnection;

        public AccountsLeaderboardDao(string stringConnection)
        {
            _myOrm = new MyOrm(stringConnection);
            _stringConnection = stringConnection;
        }

        public List<AccountsLeaderboard> Select() => _myOrm.Select<AccountsLeaderboard>();

        public AccountsLeaderboard Select(int id) => _myOrm.Select<AccountsLeaderboard>(id);

        public void Insert(params string[] args) => _myOrm.Insert<AccountsLeaderboard>(args);

        public void Insert(AccountsLeaderboard result) => _myOrm.Insert<AccountsLeaderboard>(result);

        public void Delete() => _myOrm.Delete<AccountsLeaderboard>();

        public void Delete(int id) => _myOrm.Delete<AccountsLeaderboard>(id);

        public void Update(int id, string tableName, string newValue) => _myOrm.Update<AccountsLeaderboard>(id, tableName, newValue);

        public void InsertFromAccountsTime()
        {
            Delete();

            string sqlExpression = $"insert into AccountsLeaderboard " +
                $"select a.name, min(time) as best_time from AccountsTime at join Account a on at.id = a.id " +
                $"group by at.id, a.name " +
                $"order by best_time ";

            using SqlConnection connection = new SqlConnection(_stringConnection);

            connection.Open();
            SqlCommand command = new SqlCommand(sqlExpression, connection);
            command.ExecuteNonQuery();
        }
    }
}
