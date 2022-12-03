using System.Data.SqlClient;
using System.Reflection;
using SharedLibrary.Attributes;

namespace MyORM;

public class MyOrm
{
    private readonly string _stringConnection;

    public MyOrm(string connection)
    {
        _stringConnection = connection;
    }

    public List<T> Select<T>()
    {
        var model = typeof(T);
        var selectList = new List<T>();
        
        string sqlExpression = $"SELECT * FROM {model.Name}";

        using SqlConnection connection = new SqlConnection(_stringConnection);
        
        connection.Open();
        SqlCommand command = new SqlCommand(sqlExpression, connection);
        using SqlDataReader reader = command.ExecuteReader();
        
        if (reader.HasRows)
        {
            var types = model.GetFields().Select(p => p.FieldType).ToArray();
            while (reader.Read())
            {
                var instance = GetClass<T>(model, types, reader);
                selectList.Add(instance);
            }
        }

        return selectList;
    }
    
    public T Select<T>(int id)
    {
        var model = typeof(T);

        string sqlExpression = $"select * from {model.Name} where Id = {id}";

        using SqlConnection connection = new SqlConnection(_stringConnection);
        
        connection.Open();
        SqlCommand command = new SqlCommand(sqlExpression, connection);
        using SqlDataReader reader = command.ExecuteReader();
        
        if (reader.HasRows)
        {
            var types = model.GetFields().Select(p => p.FieldType).ToArray();
            while (reader.Read())
            {
                if (reader.GetInt32(0) == id)
                    return GetClass<T>(model, types, reader);
            }
        }

        return default;
    }

    public void Insert<T>(params string[] args)
    {
        var model = typeof(T);


        string sqlExpression = $"insert into {model.Name} " + 
                               $"values ('{string.Join("', '", args)}')";
        
        using SqlConnection connection = new SqlConnection(_stringConnection);
        
        connection.Open();
        SqlCommand command = new SqlCommand(sqlExpression, connection);
        command.ExecuteNonQuery();
    }

    public void Insert<T>(T instance)
    {
        string[] args = typeof(T).GetFields().Where(p => p.GetCustomAttribute<KeyAttribute>() == null)
            .Select(p => p.GetValue(instance)?.ToString() ?? "null").ToArray();

        Console.WriteLine($"values ('{string.Join("', '", args)}')");
        Insert<T>(args);
    }

    public void Delete<T>()
    {
        var model = typeof(T);

        string sqlExpression = $"delete from {model.Name}";
        
        using SqlConnection connection = new SqlConnection(_stringConnection);
        
        connection.Open();
        SqlCommand command = new SqlCommand(sqlExpression, connection);
        command.ExecuteNonQuery();
    }
    
    public void Delete<T>(int id)
    {
        var model = typeof(T);

        string sqlExpression = $"delete from {model.Name} " + 
                               $"where Id = {id}";
        
        using SqlConnection connection = new SqlConnection(_stringConnection);
        
        connection.Open();
        SqlCommand command = new SqlCommand(sqlExpression, connection);
        command.ExecuteNonQuery();
    }

    public void Update<T>(int id, string tableName, string newValue)
    {
        var model = typeof(T);

        string sqlExpression = $"update {model.Name} " + 
                               $"set {tableName} = '{newValue}' " + 
                               $"where Id = {id}";
        
        using SqlConnection connection = new SqlConnection(_stringConnection);
        
        connection.Open();
        SqlCommand command = new SqlCommand(sqlExpression, connection);
        command.ExecuteNonQuery();
    }

    private T GetClass<T>(Type model, Type[] types, SqlDataReader reader)
    {
        var objects = types.Select((t, i) => t == typeof(string) ? reader.GetValue(i).ToString() : Convert.ChangeType(reader.GetValue(i), t)).ToArray();
        return (T)model.GetConstructor(types).Invoke(objects);
    }
}