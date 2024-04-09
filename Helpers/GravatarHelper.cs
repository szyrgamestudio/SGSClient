using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SGSClient.Helpers;
public static class GravatarHelper
{
    public static string GetAvatarUrl(string email, int size = 80)
    {
        email = email.Trim().ToLower();

        using (MD5 md5 = MD5.Create())
        {
            byte[] emailBytes = Encoding.UTF8.GetBytes(email);
            byte[] hashBytes = md5.ComputeHash(emailBytes);

            // Konwertuj bajty na stringa heksadecymalnego
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }

            // Wygeneruj URL awatara
            return $"https://www.gravatar.com/avatar/{sb.ToString()}?s={size}&d=mp";
        }
    }

    public static void DownloadAvatar(string email, string savePath)
    {
        string avatarUrl = GetAvatarUrl(email);

        using (WebClient client = new WebClient())
        {
            client.DownloadFile(new Uri(avatarUrl), savePath);
        }
    }
}