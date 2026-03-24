namespace RimKeeperModOrganizerLib.Helpers;

public static class ModHelper
{
    public static string? ShortVersion(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        int firstDot = input.IndexOf('.');
        if (firstDot < 0) return null;
        int secondDot = input.IndexOf('.', firstDot + 1);
        if (secondDot < 0)
        {
            var minorPart = new string(input
                .Skip(firstDot + 1)
                .TakeWhile(char.IsDigit)
                .ToArray());

            if (minorPart.Length == 0) return null;

            var majorPart = new string(input
                .Take(firstDot)
                .Reverse()
                .TakeWhile(char.IsDigit)
                .Reverse()
                .ToArray());

            return majorPart.Length > 0 ? $"{majorPart}.{minorPart}" : null;
        }
        return input.Substring(0, secondDot);
    }
}
