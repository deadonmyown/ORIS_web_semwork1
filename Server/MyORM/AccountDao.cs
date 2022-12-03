using SharedLibrary.Models;

namespace MyORM;

public class AccountDao
{
    private readonly MyOrm _myOrm; 

    public AccountDao(string connectionString)
    {
        _myOrm = new MyOrm(connectionString);
    }

    public List<Account> Select() => _myOrm.Select<Account>();

    public Account Select(int id) => _myOrm.Select<Account>(id);

    public void Insert(params string[] args) => _myOrm.Insert<Account>(args);

    public void Insert(Account account) => _myOrm.Insert<Account>(account);

    public void Delete() => _myOrm.Delete<Account>();

    public void Delete(int id) => _myOrm.Delete<Account>(id);

    public void Update(int id, string tableName, string newValue) => _myOrm.Update<Account>(id, tableName, newValue);
}