namespace tero.session.src.Core;

public static class CoreExtensions
{
    public static List<T> Shuffle<T>(this List<T> list)
    {
        var random = new Random();

        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = random.Next(n + 1);
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }
}