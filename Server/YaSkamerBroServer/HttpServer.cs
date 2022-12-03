using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;
using System.Threading.Tasks;
using GameServer.Controllers;

namespace GameServer
{
    public class HttpServer : IDisposable
    {
        private static object _locker = new object();

        public RouteTree RouteTree { get; }

        public bool AdminRules { get; set; } = false;

        public ServerStatus ServerStatus { get; private set; } = ServerStatus.Stop;

        private readonly HttpListener _httpListener;

        private volatile ServerSettings _serverSettings;

        private IDictionary<string, (Type, object)> controllers;
        
        private IDictionary<string, string> paths;

        private Task listeningTask;

        public HttpServer()
        {
            _httpListener = new HttpListener();
            RouteTree = new RouteTree();
            controllers = new Dictionary<string, (Type, object)>();
            RegisterControllers();
            paths = new Dictionary<string, string>();
            ServerFileHandling.ProcessDirectory(".\\static", paths);
        }

        public void Start()
        {
            if (ServerStatus == ServerStatus.Start)
            {
                Console.WriteLine("Сервер уже был запущен");
                return;
            }

            _serverSettings = ServerFileHandling.ReadJsonSettings("./settings.json");

            _httpListener.Prefixes.Clear();
            _httpListener.Prefixes.Add($"http://localhost:{_serverSettings.Port}/");

            Console.WriteLine("запускаем скам сервер");
            _httpListener.Start();
            ServerStatus = ServerStatus.Start;
            Console.WriteLine("скам машина запущена");

            listeningTask = Listening();
        }

        public void Stop()
        {
            if (ServerStatus == ServerStatus.Stop)
            {
                Console.WriteLine("Сервер уже выключен");
                return;
            }

            Console.WriteLine("останавливаем скам сервер");
            ServerStatus = ServerStatus.Stop;
            lock (_locker)
                _httpListener.Stop();
            try
            {
                listeningTask.Wait();
                Console.WriteLine("Task waited");
            }
            catch
            {
                Console.WriteLine("listeningTask can't dispose");
            }

            Console.WriteLine("скам прекращен");
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        private async Task Listening()
        {
            while (ServerStatus == ServerStatus.Start)
            {
                try
                {
                    HttpListenerContext context = await _httpListener.GetContextAsync();

                    if (!(await MethodHandler(context)))
                        FileSiteHandler(context);
                    //await MethodHandler(context);
                    //FileSiteHandler(context);
                }
                catch (HttpListenerException e)
                {
                    Console.WriteLine("HttpListenerException, которое непонятно как пофиксить без try-catch");
                }
            }
        }

        private string DefineContentType(string format)
        {
            return format switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".png" => "image/png",
                ".svg" => "image/svg+xml",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".ico" => "image/vnd.microsoft.icon",
                _ => "text/plain"
            };
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

        private void FileSiteHandler(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            byte[] buffer;
            string format;
            if (Directory.Exists(_serverSettings.Path))
            {
                //string url = request.RawUrl.Replace("%20", " ");
                string filename = context.Request.Url.Segments[^1].Replace("%20", " ");
                filename = filename == "/" ? "" : filename; 
                (buffer, format) =
                    ServerFileHandling.GetFileStatic(filename, _serverSettings, paths);
                response.Headers.Set("Content-Type", DefineContentType(format));
                if (buffer == null)
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    string err = "404 - not found";

                    buffer = Encoding.UTF8.GetBytes(err);
                }
            }
            else
            {
                string err = $"Directory {_serverSettings.Path} not found";
                buffer = Encoding.UTF8.GetBytes(err);
            }

            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        private async Task<bool> MethodHandler(HttpListenerContext _httpContext)
        {
            var request = _httpContext.Request;
            var response = _httpContext.Response;

            string controllerName = "";
            
            if (_httpContext.Request.Url.Segments.Length >= 2) controllerName = _httpContext.Request.Url.Segments[1].Replace("/", "");;

            if (!controllers.ContainsKey(controllerName)) return false;
            var (controller, instance) = controllers[controllerName];
            if (controller.BaseType == typeof(Controller))
            {
                foreach (var property in controller.BaseType.GetProperties())
                {
                    if (property.PropertyType == typeof(HttpListenerContext))
                    {
                        property.SetValue(instance, _httpContext);
                    }
                }
            }


            string bodyRet = null;
            IDictionary<string, string> bodyParams = null;
            if (request.HasEntityBody)
            {
                Stream body = request.InputStream;
                Encoding encoding = request.ContentEncoding;
                StreamReader reader = new StreamReader(body, encoding);
                bodyRet = reader.ReadToEnd();
                bodyParams = bodyRet.ParseAsQuery(true);
            }

            var res = await RouteTree.TryNavigate(AdminRules ? GetHttpMethod() : new HttpMethod(request.HttpMethod),
                request.RawUrl, bodyRet, null, bodyParams);

            if (!res.Item1) return false;

            byte[] buffer = null;
            if (res.Item2 is Html)
            {
                response.ContentType = "text/html";
                buffer = Encoding.UTF8.GetBytes(((Html)res.Item2).Page);
            }
            else
            {
                response.ContentType = "Application/json";
                buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res.Item2));
            }

            response.ContentLength64 = buffer.Length;

            response.OutputStream.Write(buffer, 0, buffer.Length);

            response.Close();

            return true;
        }

        private void RegisterControllers()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var controllers = assembly.GetTypes()
                .Where(t => Attribute.IsDefined(t, typeof(HttpController)))
                .Select(type => (type, type.GetCustomAttribute<HttpController>(), type.BaseType))
                .Select(tuple => (tuple.type, tuple.Item2.ControllerName == null
                    ? tuple.type.Name.ToLower().Replace("controller", "")
                    : tuple.Item2.ControllerName, tuple.BaseType));

            StringBuilder route = new StringBuilder();
            foreach (var controller in controllers)
            {
                var instance = controller.type.GetConstructor(new Type[] { }).Invoke(new object[] { });
                RouteTree.RegisterCaller(controller.type, instance);

                string controllerName = controller.Item2;
                this.controllers[controllerName] = (controller.type, instance);

                foreach (var method in controller.type.GetMethods())
                {
                    var attr = method.GetCustomAttribute<HttpMethodAttribute>();
                    if (attr == null)
                        continue;

                    route.Clear();
                    route.Append($"/{controller.Item2}");
                    route.Append($"/{attr.MethodUri}");

                    RouteTree.AddRoute(attr.HttpMethod, route.ToString(), controller.type, method);
                }
            }
        }

        private HttpMethod GetHttpMethod()
        {
            Console.WriteLine("Type command and after http method");
            var method = Console.ReadLine()?.ToUpper();
            return method switch
            {
                "GET" or "POST" or "DELETE" or "UPDATE" => new HttpMethod(method),
                _ => new HttpMethod("GET")
            };
        }
    }
}
