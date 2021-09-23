using System;
using System.Text.RegularExpressions;

namespace PolyChat.Util
{
    static class IP
    {
        private const string REGEX_IP = @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)(\.(?!$)|$)){4}$";

        public static string GetIPFromCode(string code)
        {
            return code;
        }

        public static string GetCodeFromIP(string ip)
        {
            string[] arr = ip.Split('.');
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = int.Parse(arr[i]).ToString("X");
                Console.WriteLine(arr[i]);
            }
            return ip;
        }

        public static bool ValidateIP(string ip)
        {
            return Regex.IsMatch(ip, REGEX_IP);
        }
    }
}
