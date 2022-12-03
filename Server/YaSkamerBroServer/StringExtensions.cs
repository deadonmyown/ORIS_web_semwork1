using System.Globalization;

namespace GameServer;

public static class StringExtensions
{
    public static IDictionary<string, string> ParseAsQuery(this string str, bool firstCharToLowerCase = false)
        => str.Split("&")
            .Select(s => s.Split("="))
            .Where(arr => arr.Length == 2)
            .ToDictionary(key => Uri.UnescapeDataString(firstCharToLowerCase ? key[0].FirstCharToLowerCase() : key[0]),
                val => Uri.UnescapeDataString(val[1]));
    

    public static string[] ParseAsQueryToArray(this string str)
        => str.Split("&")
            .Select(s => s.Split("="))
            .Where(arr => arr.Length == 2)
            .Select(arr => arr[1]).ToArray();
    
    
    public static string? FirstCharToLowerCase(this string? str)
    {
        if ( !string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
            return str.Length == 1 ? char.ToLower(str[0]).ToString() : char.ToLower(str[0]) + str[1..];

        return str;
    }
}