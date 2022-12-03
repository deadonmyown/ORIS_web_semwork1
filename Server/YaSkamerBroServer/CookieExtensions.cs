using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace GameServer
{
    public static class CookieExtensions
    {
        /*public static Guid CheckCookieName(this CookieCollection cookieCollection, string name)// => cookieCollection[name] != null ? true : false;
        {
            var cookie = cookieCollection[name];
            if (cookie != null && Guid.TryParse(cookie.Value, out Guid userGuid))
                return userGuid;
            return Guid.Empty;
        }*/

        public static bool CheckSessionId(this CookieCollection cookieCollection)
        {
            var cookie = cookieCollection["SessionId"];
            if (cookie != null)
            {
                if(Guid.TryParse(cookie.Value, out Guid userGuid) && SessionManager.CheckSession(userGuid))
                {
                    return true;
                }
            }
            return false;
        }

        public static int? CheckAuthorizedAccount(this CookieCollection cookieCollection)
        {
            var cookie = cookieCollection["SessionId"];
            if (cookie != null)
            {
                if(Guid.TryParse(cookie.Value, out Guid userGuid) && SessionManager.CheckSession(userGuid))
                {
                    return SessionManager.GetSession(userGuid).AccountId;
                }
            }
            return null;
        }
    }
}
