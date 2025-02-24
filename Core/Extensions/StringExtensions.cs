namespace Core.Extensions;

public static class StringExtensions
{
    public static bool IsJsonObject(this string json)
    {
        return json.Length > 0 && json[0] == '{';
    }

    public static bool IsArray(this string json)
    {
        return json.Length > 0 && json[0] == '[';
    }
}