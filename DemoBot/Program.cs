using System;
using System.Net;
using System.Security;

namespace DemoBot
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            const string email = "emily@socialcast.com";
            const string plainPassword = "demo";
            const string userAgent = "My bot/1.0";
            const string communityLocation = "https://demo.socialcast.com";

            var password = new SecureString();
            foreach (var letter in plainPassword) password.AppendChar(letter);
            password.MakeReadOnly();

            var api = new SocialcastBot.Api(new Uri(communityLocation), userAgent, email, password, SocialcastBot.ApiResponseFormat.Json);
            // api.SetProxy(new WebProxy("proxy.example.org", 8080));

            Console.WriteLine(api.Authenticate(email, plainPassword));
            Console.WriteLine(api.GetUsers());
            Console.WriteLine(api.ShowUser("1"));

            Console.ReadLine();
        }
    }
}
