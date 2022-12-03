using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLibrary.Models;

namespace MyORM
{
    public class UpdatesDao
    {
        private readonly MyOrm _myOrm;

        public UpdatesDao(string connectionString)
        {
            _myOrm = new MyOrm(connectionString);
        }

        public List<Updates> Select() => _myOrm.Select<Updates>();

        public Updates Select(int id) => _myOrm.Select<Updates>(id);

        public void Insert(params string[] args) => _myOrm.Insert<Updates>(args);

        public void Insert(Updates forum) => _myOrm.Insert<Updates>(forum);

        public void Delete() => _myOrm.Delete<Updates>();

        public void Delete(int id) => _myOrm.Delete<Updates>(id);

        public void Update(int id, string tableName, string newValue) => _myOrm.Update<Updates>(id, tableName, newValue);
    }
}
