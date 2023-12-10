using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace PresalesApp.Service.Extensions;

public static class StringExtensions
{
    public static string CreateMD5(this string input)
    {
        var inputBytes = Encoding.ASCII.GetBytes(input);
        var hashBytes = MD5.HashData(inputBytes);
        var sb = new StringBuilder();

        for(var i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("X2", CultureInfo.CurrentCulture));
        }

        return sb.ToString();
    }
}
