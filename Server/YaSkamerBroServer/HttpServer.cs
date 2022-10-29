using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace YaSkamerBroServer
{
    public class HttpServer : IDisposable
    {
        private static object _locker = new object();
        public ServerStatus ServerStatus { get; private set; } = ServerStatus.Stop;
        
        private readonly HttpListener _httpListener;
        
        private volatile ServerSettings _serverSettings;

        private Task listeningTask;
        
        public HttpServer()
        {
            _httpListener = new HttpListener();
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
            lock(_locker)
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
                    lock (_locker)
                    {
                        MethodHandler(context);
                        FileSiteHandler(context);
                    }
                }
                catch(HttpListenerException e)
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
                (buffer, format) =
                    ServerFileHandling.GetFile(request.RawUrl.Replace("%20", " "), _serverSettings);
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

        private bool MethodHandler(HttpListenerContext _httpContext)
        {
            // объект запроса
            HttpListenerRequest request = _httpContext.Request;

            // объект ответа
            HttpListenerResponse response = _httpContext.Response;

            if (_httpContext.Request.Url.Segments.Length < 2) return false;

            string controllerName = _httpContext.Request.Url.Segments[1].Replace("/", "");

            string[] strParams = _httpContext.Request.Url
                                    .Segments
                                    .Skip(2)
                                    .Select(s => s.Replace("/", ""))
                                    .ToArray();

            var assembly = Assembly.GetExecutingAssembly();

            var controller = assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(HttpController))).FirstOrDefault(c => c.Name.ToLower() == controllerName.ToLower());

            if (controller == null) return false;

            var test = typeof(HttpController).Name;
            var method = controller.GetMethods().Where(t => t.GetCustomAttributes(true)
                                                              .Any(attr => attr.GetType().Name == $"Http{_httpContext.Request.HttpMethod}"))
                                                 .LastOrDefault();
            
            if (method == null) return false;

            object[] queryParams = method.GetParameters()
                                .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
                                .ToArray();

            var ret = method.Invoke(Activator.CreateInstance(controller), queryParams);

            response.ContentType = "Application/json";

            byte[] buffer = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret));
            response.ContentLength64 = buffer.Length;

            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            output.Close();
            
            return true;
        }
    }
}
