namespace test.Utilities;

public static class StringGenerator
{
    private static readonly Random Random = new Random();
    private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string Generate(int length)
    {
        return new string(Enumerable.Repeat(Chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
}