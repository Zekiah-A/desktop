using System.Text;

namespace OpenMc2D.Networking;

public static class NetworkingHelpers
{
    public static string EncodeURIComponent(string uri)
    {
        const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.!~*'()";

        var result = new StringBuilder();

        foreach (var @char in uri)
        {
            if (allowedChars.IndexOf(@char) != -1)
            {
                result.Append(@char);
            }
            else
            {
                result.Append('%' + ((int)@char).ToString("X2"));
            }
        }

        return result.ToString();
    }
}