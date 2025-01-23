namespace PresalesApp.Extensions;

public static class EnumExtension
{
    // https://stackoverflow.com/a/643438
    public static T Next<T>(this T src, int skipFirst = 0) where T : struct
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException(string.Format("Argument {0} is not an Enum",
                typeof(T).FullName));
        }

        var Arr = (T[])Enum.GetValues(src.GetType());
        var j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[skipFirst] : Arr[j];
    }
}
