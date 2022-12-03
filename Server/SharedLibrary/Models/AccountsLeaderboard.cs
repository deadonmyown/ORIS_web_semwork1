using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLibrary.Attributes;

namespace SharedLibrary.Models
{
    public class AccountsLeaderboard
    {
        [Key]
        public int Place;
        public string Name;
        public string BestTime;

        public AccountsLeaderboard(int place, string name, string bestTime) : this(name, bestTime)
        {
            Place = place;
        }

        public AccountsLeaderboard(string name, string bestTime) {
            Name = name;
            BestTime = bestTime;
        }

        public AccountsLeaderboard() { }
    }
}
