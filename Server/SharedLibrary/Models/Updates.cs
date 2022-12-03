using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLibrary.Attributes;

namespace SharedLibrary.Models
{
    public class Updates
    {
        [Key]
        public int Id;
        public string Theme;
        public string AccountId;
        public string AccountName;
        public string Date;

        public Updates() { }

        public Updates(string theme, string accountId, string accountName, string date)
        {
            Theme = theme;
            AccountId = accountId;
            AccountName = accountName;
            Date = date;
        }

        public Updates(int id, string theme, string accountId, string accountName, string date) : this(theme, accountId, accountName, date)
        {
            Id = id;
        }
    }
}
