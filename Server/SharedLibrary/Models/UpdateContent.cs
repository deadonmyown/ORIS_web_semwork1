using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models
{
    public class UpdateContent
    {
        public int UpdateId;
        public string AccountId;
        public string Content;
        public string ContentDate;

        public UpdateContent() { }

        public UpdateContent(int updateId, string accountId, string content, string contentDate)
        {
            UpdateId = updateId;
            AccountId = accountId;
            Content = content;
            ContentDate = contentDate;
        }
    }
}
