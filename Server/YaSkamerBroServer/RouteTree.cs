using System.Reflection;

namespace GameServer;

public static class IDictionaryExtensions
{
    public static bool TryGet<T, U>(this IDictionary<T, U> dict, T key, out U val, U defaultVal = default)
    {
        if (key != null && (dict?.ContainsKey(key)).GetValueOrDefault())
        {
            val = dict[key];
            return true;
        }

        val = defaultVal;
        return false;
    }
}

public class RouteTree
{
    private record Method(MethodInfo MethodInfo, Type CallerType);
    
    private class RouteTreeNode
    {
        public string ArgName { get; set; }
        public ArgType ArgType { get; set; }
        public IDictionary<HttpMethod, Method> Functions { get; set; }

        private IDictionary<string, RouteTreeNode> PlainSubRoutes { get; set; }
        private IDictionary<Type, RouteTreeNode> ArgSubRoutes { get; set; }
        
        public RouteTreeNode() {}

        public RouteTreeNode(string argName, ArgType argType)
        {
            ArgName = argName;
            ArgType = argType;
        }

        public Method this[HttpMethod key]
        {
            set
            {
                if (Functions == null)
                    Functions = new Dictionary<HttpMethod, Method>();
                Functions[key] = value;
            }
        }

        public bool TryGetFunction(HttpMethod httpMethod, out Method function) =>
            Functions.TryGet(httpMethod, out function);

        public RouteTreeNode this[string key]
        {
            set
            {
                if (PlainSubRoutes == null)
                    PlainSubRoutes = new Dictionary<string, RouteTreeNode>();
                PlainSubRoutes[key] = value;
            }
        }

        public bool TryGetPlainSubRoute(string key, out RouteTreeNode routeTreeNode) =>
            PlainSubRoutes.TryGet(key, out routeTreeNode, this);
        
        public RouteTreeNode this[Type key]
        {
            set
            {
                if (ArgSubRoutes == null)
                    ArgSubRoutes = new Dictionary<Type, RouteTreeNode>();
                ArgSubRoutes[key] = value;
            }
        }
        
        public bool TryGetArgSubRoute(Type key, out RouteTreeNode routeTreeNode) =>
            ArgSubRoutes.TryGet(key, out routeTreeNode, this);
    }

    private RouteTreeNode root;

    private IDictionary<Type, object> callers;

    public void RegisterCaller(Type callerType, object instance)
    {
        if (callers == null)
            callers = new Dictionary<Type, object>();

        callers[callerType] = instance;
    }

    public void AddRoute(HttpMethod method, string route, Type callerType, MethodInfo function)
    {
        if(root == null)
            root = new RouteTreeNode();

        RouteTreeNode currentNode = root;
        string[] splitRoute = route.Split("/", StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < splitRoute.Length; i++)
        {
            RouteTreeNode nextNode = null;

            if (splitRoute[i].StartsWith("{") && splitRoute[i].EndsWith("}"))
            {
                string typeStr = splitRoute[i].Substring(1, splitRoute[i].Length - 2);
                int index = typeStr.IndexOf(":");
                string name = String.Empty;
                if (index == -1)
                {
                    name = typeStr;
                }
                else
                {
                    name = typeStr.Substring(0, index);
                    typeStr = typeStr.Substring(index + 1);
                }
                
                ArgType argType = ArgType.GetArgType(typeStr);
                nextNode = currentNode.TryGetArgSubRoute(argType.Type, out RouteTreeNode rtn)
                    ? rtn
                    : new RouteTreeNode(name, argType);
                currentNode[argType.Type] = nextNode;
            }
            else
            {
                nextNode = currentNode.TryGetPlainSubRoute(splitRoute[i], out RouteTreeNode rtn)
                    ? rtn
                    : new RouteTreeNode();
                currentNode[splitRoute[i]] = nextNode;
            }
            
            currentNode = nextNode;
        }
        
        currentNode[method] = new Method(function, callerType);
    }

    public async Task<(bool, object, Type)> TryNavigate(HttpMethod method, string route, string body = null,
        byte[] bodyBuffer = null, IDictionary<string, string> extraParams = null, bool nameRequired = true)
    {
        if (root == null)
            return (false, null, null);

        IDictionary<string, string> queryParams = null;
        int index = route.IndexOf("?");
        if (index != -1)
        {
            queryParams = route.Substring(index + 1).ParseAsQuery();
            route = route.Substring(0, index);
        }

        if (extraParams != null)
        {
            if (queryParams == null)
            {
                queryParams = extraParams;
            }
            else
            {
                foreach (var param in extraParams)
                {
                    queryParams[param.Key] = param.Value;
                }
            }
        }
        
        if(queryParams == null)
        {
            queryParams = new Dictionary<string, string>();
        }

        RouteTreeNode currentNode = root;
        IDictionary<string, object> routeArgs = new Dictionary<string, object>();
        string[] splitRoute = route.Split("/", StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < splitRoute.Length; i++)
        {
            if(currentNode.TryGetPlainSubRoute(splitRoute[i], out currentNode))
                continue;
            else if (ArgType.TryParse(splitRoute[i], out object val, out Type type) &&
                     currentNode.TryGetArgSubRoute(type, out currentNode))
            {
                routeArgs[currentNode.ArgName] = val;
            }
            else
            {
                return (false, null, null);
            }
        }

        if (currentNode.TryGetFunction(method, out Method function))
        {
            IList<object> argList = new List<object>();
            foreach (var param in function.MethodInfo.GetParameters())
            {
                if (param.ParameterType == typeof(byte[]))
                {
                    argList.Add(bodyBuffer);
                    continue;
                }

                if (routeArgs.TryGetValue(param.Name, out object val))
                {
                    argList.Add(val);
                    continue;
                }

                if (!queryParams.TryGetValue(param.Name, out string valueStr))
                {
                    if (nameRequired)
                        valueStr = body;
                    else
                        valueStr = queryParams[param.Name];
                }

                if (!string.IsNullOrEmpty(valueStr))
                {
                    if (ArgType.TryParse(param.ParameterType, valueStr, out object value))
                    {
                        argList.Add(value);
                        continue;
                    }
                    
                    //Newtonsoft.Json lib required
                    /*try
                    {
                        try JSON deserializing value
                        argList.Add(
                            JsonConvert.DeserializeObject(valueStr, param.ParameterType));
                        continue;
                    }
                    catch { }*/
                }
                argList.Add(param.HasDefaultValue ? param.DefaultValue : null);
            }

            if (!callers.TryGet(function.CallerType, out object caller))
            {
                return (false, null, null);
            }
            object ret = function.MethodInfo.Invoke(caller, argList.ToArray());
            Type retType = function.MethodInfo.ReturnType;
            if (retType == typeof(Task))
            {
                // no return from task
                ret = null;
                retType = null;
            }
            else if (retType.IsGenericType
                     && retType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                // await task completion
                Task task = ret as Task;
                await task.ConfigureAwait(false);

                // get return from task
                ret = task.GetType().GetProperty(nameof(Task<object>.Result)).GetValue(task);
                retType = ret.GetType();
            }
            return (true, ret, retType);
        }

        return (false, null, null);
    }
}