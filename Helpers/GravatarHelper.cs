using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SGSClient.Helpers
{
    public static class GravatarHelper
    {
        public static string GetAvatarUrl(string email, int size = 80)
        {
            email = email.Trim().ToLower();

            using (MD5 md5 = MD5.Create())
            {
                byte[] emailBytes = Encoding.UTF8.GetBytes(email);
                byte[] hashBytes = md5.ComputeHash(emailBytes);

                // Convert bytes to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                // Generate avatar URL
                return $"https://www.gravatar.com/avatar/{sb.ToString()}?s={size}&d=mp";
            }
        }

        public static async Task DownloadAvatarAsync(string email, string savePath)
        {
            string avatarUrl = GetAvatarUrl(email);

            using (WebClient client = new WebClient())
            {
                // Download the avatar asynchronously
                await client.DownloadFileTaskAsync(new Uri(avatarUrl), savePath);
            }
        }
    }
}
