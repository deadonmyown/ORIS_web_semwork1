using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLibrary.Models;

namespace MyORM
{
    public class UpdateContentDao
    {
        private readonly MyOrm _myOrm;
        private readonly string _stringConnection;

        public UpdateContentDao(string stringConnection)
        {
            _myOrm = new MyOrm(stringConnection);
            _stringConnection = stringConnection;
        }

        public List<UpdateContent> Select() => _myOrm.Select<UpdateContent>();

        public UpdateContent Select(int id) => _myOrm.Select<UpdateContent>(id);

        public void Insert(params string[] args) => _myOrm.Insert<UpdateContent>(args);

        public void Insert(UpdateContent comment) => _myOrm.Insert<UpdateContent>(comment);

        public void Delete() => _myOrm.Delete<UpdateContent>();

        public void Delete(int id) => _myOrm.Delete<UpdateContent>(id);

        public void Update(int id, string tableName, string newValue) => _myOrm.Update<UpdateContent>(id, tableName, newValue);

        public void UpdateInEditMode(int id, string newValue)
        {
            string sqlExpression = $"update UpdateContent " +
                               $"set content = '{newValue}' " +
                               $"where update_id = {id}";

            using SqlConnection connection = new SqlConnection(_stringConnection);

            connection.Open();
            SqlCommand command = new SqlCommand(sqlExpression, connection);
            command.ExecuteNonQuery();
        }
    }
}
