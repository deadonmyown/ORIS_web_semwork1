using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLibrary.Attributes;

namespace SharedLibrary.Models
{
    public class Account
    {
        [Key]
        public int Id;
        public string Name;
        public string Email;
        public string Phone;
        public string Password;

        public Account() { }

        public Account(string name, string email, string phone, string password)
            => (Name, Email, Phone, Password) = (name, email, phone, password);

        public Account(int id, string name, string email, string phone, string password) : this(name, email, phone, password)
            => Id = id;
    }
}
