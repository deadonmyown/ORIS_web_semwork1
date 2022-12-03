/*using MyORM.Attributes;

namespace MyORM.Model;

public class Account
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Password { get; set; }

    public Account() { }

    public Account(string name, string email, string phone, string password)
        => (Name, Email, Phone, Password) = (name, email, phone, password);

    public Account(int id, string name, string email, string phone, string password) : this(name, email, phone, password)
        => Id = id;
}*/