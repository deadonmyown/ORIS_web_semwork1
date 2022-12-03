using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLibrary.Models;

namespace MyORM
{
    public class AccountsTimeDao
    {
        private readonly MyOrm _myOrm;

        public AccountsTimeDao(string connectionString)
        {
            _myOrm = new MyOrm(connectionString);
        }

        public List<AccountsTime> Select() => _myOrm.Select<AccountsTime>();

        public AccountsTime Select(int id) => _myOrm.Select<AccountsTime>(id);

        public void Insert(params string[] args) => _myOrm.Insert<AccountsTime>(args);

        public void Insert(AccountsTime accountTime) => _myOrm.Insert<AccountsTime>(accountTime);

        public void Delete() => _myOrm.Delete<AccountsTime>();

        public void Delete(int id) => _myOrm.Delete<AccountsTime>(id);

        public void Update(int id, string tableName, string newValue) => _myOrm.Update<AccountsTime>(id, tableName, newValue);
    }
}
