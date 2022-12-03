using SharedLibrary.Models;

namespace MyORM;

public class AccountRepository
{
    private readonly Dictionary<int, Account> _repository;
    private readonly MyOrm _myOrm;

    public AccountRepository(string connectionString)
    {
        _myOrm = new MyOrm(connectionString);
        _repository = _myOrm.Select<Account>().ToDictionary(key => key.Id, value => value);
    }
    
    public List<Account> Select() => _repository.Values.ToList();

    public Account Select(int id) => _repository[id];

    public void Insert(params string[] args)
    {
        _myOrm.Insert<Account>(args);
        Account account = _myOrm.Select<Account>()
            .First(a => args.All(arg => a.GetType().GetFields().Any(p => p.GetValue(a)?.ToString() == arg)));
        _repository[account.Id] = account;
    }

    public void Delete()
    {
        _repository.Clear();
        _myOrm.Delete<Account>();
    }

    public void Delete(int id)
    {
        _repository.Remove(id);
        _myOrm.Delete<Account>(id);
    }

    public void Update(int id, string tableName, string newValue)
    {
        _myOrm.Update<Account>(id, tableName, newValue);
        var account = _repository[id];
        var field = account.GetType().GetField(tableName);
        field?.SetValue(account, Convert.ChangeType(newValue, field.FieldType));
    }
}