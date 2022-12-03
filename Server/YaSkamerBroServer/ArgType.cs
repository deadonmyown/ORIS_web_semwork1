namespace GameServer;

public class ArgType
{
    private static readonly IList<ArgType> ArgTypes = new List<ArgType>()
    {
        new ArgType(typeof(string), s => s, "str"),
        new ArgType(typeof(Guid), s => Guid.Parse(s)),
        new ArgType(typeof(double), s => Double.Parse(s), "float"),
        new ArgType(typeof(int), s => int.Parse(s), "int")
        //new ArgType(typeof(bool), s => bool.Parse(s), "bool")
    };

    public static void RegisterType<T>(Func<string, object> parser, params string[] altNames)
    {
        ArgType? existArgType = ArgTypes.SingleOrDefault(a => a.Type == typeof(T));
        if (existArgType != null)
        {
            existArgType.Parser = parser;
            existArgType.AltNames.Concat(altNames);
        }
        else
        {
            ArgTypes.Add(new ArgType(typeof(T), parser, altNames));
        }
    }

    public static bool TryParse(string valStr, out object? val, out Type? type)
    {
        for (int i = ArgTypes.Count - 1; i >= 0; i--)
        {
            ArgType argType = ArgTypes[i];
            try
            {
                val = argType.Parser(valStr);
                type = argType.Type;
                return true;
            }
            catch
            {
                continue;
            }
        }

        val = null;
        type = null;
        return false;
    }

    public static bool TryParse<T>(string valStr, out T val)
    {
        val = default;
        if (TryParse(typeof(T), valStr, out object objVal))
        {
            val = (T)objVal;
            return true;
        }

        return false;
    }

    public static bool TryParse(Type type, string valStr, out object val)
    {
        try
        {
            val = GetArgType(type).Parser(valStr);
            return true;
        }
        catch
        {
            val = default;
            return false;
        }
    }

    public static ArgType? GetArgType(Type type) => ArgTypes.SingleOrDefault(a => a.Type == type);

    public static ArgType? GetArgType(string argtypeStr)
    {
        argtypeStr = argtypeStr.ToLower();
        return ArgTypes.FirstOrDefault(a => a.AltNames.Contains(argtypeStr));
    }

    public Type Type { get; private set; }
    public Func<string, object> Parser { get; private set; }
    public  HashSet<string> AltNames { get; private set; }

    private ArgType(Type type, Func<string, object> parser, params string[] altNames)
    {
        Type = type;
        Parser = parser;
        
        AltNames = new HashSet<string>();
        
        for (int i = 0; i < altNames.Length; i++)
            AltNames.Add(altNames[i].ToLower());
        AltNames.Add(type.Name.ToLower());
    }
}