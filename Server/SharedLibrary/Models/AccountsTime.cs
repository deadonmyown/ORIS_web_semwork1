using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models
{
    public class AccountsTime
    {
        public int Id;
        public string Time;

        public AccountsTime() { }

        public AccountsTime(int id, string time)
        {
            Id = id;
            Time = time;
        }

    }
}
