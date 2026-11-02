using System.Globalization;

namespace PeliculasAspnet.Services;

public static class QueryNumbers
{
    public static int OrDefault(string? value, int fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) || number == 0)
        {
            return fallback;
        }

        return number;
    }
}
