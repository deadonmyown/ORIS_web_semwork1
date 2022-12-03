using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace GameServer.Controllers
{
    public abstract class Controller
    {
        public HttpListenerContext HttpContext { get; set; }

    }
}
