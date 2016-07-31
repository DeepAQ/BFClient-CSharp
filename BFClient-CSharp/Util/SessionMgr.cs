using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using BFClient_CSharp.Properties;
using Newtonsoft.Json.Linq;

namespace BFClient_CSharp.Util
{
    internal static class SessionMgr
    {
        public static string Host = "http://localhost:8081";
        public static string Username = "";
        private static string _pwdhash = "";
        private static string _sessionId = "";

        // User login & logout
        public static void Login(string username, string password)
        {
            LoginWithPwdhash(username, Hash(password));
        }

        public static void LoginWithPwdhash(string username, string pwdhash)
        {
            var serverResp = GetUrl($"{Host}/user/login?username={username}&pwdhash={pwdhash}");
            var jsonObj = JObject.Parse(serverResp);
            if ((int) jsonObj["result"] < 0)
                throw new Exception((string) jsonObj["errmsg"]);
            _sessionId = (string) jsonObj["sessid"];
            Username = username;
            _pwdhash = pwdhash;
        }

        public static void SaveLoginInfo(string username, string password)
        {
            Settings.Default.host = Host;
            Settings.Default.username = username;
            Settings.Default.pwdhash = Hash(password);
            Settings.Default.Save();
        }

        public static bool TryAutoLogin()
        {
            Host = Settings.Default.host;
            var username = Settings.Default.username;
            var pwdhash = Settings.Default.pwdhash;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pwdhash)) return false;
            try
            {
                LoginWithPwdhash(username, pwdhash);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void RefreshSession()
        {
            LoginWithPwdhash(Username, _pwdhash);
        }

        public static void Logout()
        {
            Username = "";
            _sessionId = "";
            Settings.Default.username = "";
            Settings.Default.pwdhash = "";
            Settings.Default.Save();
        }

        public static void ChangePassword(string oldPassword, string newPassword)
        {
            var serverResp =
                GetUrl(
                    $"{Host}/user/changepassword?sessid={_sessionId}&oldpwdhash={Hash(oldPassword)}&newpwdhash={Hash(newPassword)}");
            var jsonObj = JObject.Parse(serverResp);
            if ((int) jsonObj["result"] < 0)
                throw new Exception((string) jsonObj["errmsg"]);
        }

        // File I/O
        public static string FileList() => GetUrl($"{Host}/io/list?sessid={_sessionId}");

        public static string FileContent(string filename, string version)
        {
            var serverResp = GetUrl($"{Host}/io/open?sessid={_sessionId}&filename={filename}&version={version}");
            var jsonObj = JObject.Parse(serverResp);
            if ((int) jsonObj["result"] < 0)
                throw new Exception((string) jsonObj["errmsg"]);
            return (string) jsonObj["code"];
        }

        public static string SaveFile(string code, string filename)
        {
            var encCode = HttpUtility.UrlEncode(HttpUtility.UrlEncode(code));
            var serverResp = GetUrl($"{Host}/io/save?sessid={_sessionId}&code={encCode}&filename={filename}");
            var jsonObj = JObject.Parse(serverResp);
            if ((int) jsonObj["result"] < 0)
                throw new Exception((string) jsonObj["errmsg"]);
            return (string) jsonObj["version"];
        }

        public static string[] Execute(string code, string input)
        {
            var encCode = HttpUtility.UrlEncode(HttpUtility.UrlEncode(code));
            var encInput = HttpUtility.UrlEncode(HttpUtility.UrlEncode(input));
            var serverResp = GetUrl($"{Host}/io/execute?sessid={_sessionId}&code={encCode}&input={encInput}");
            var jsonObj = JObject.Parse(serverResp);
            if ((int) jsonObj["result"] < 0)
                throw new Exception((string) jsonObj["errmsg"]);
            return new[] {(string) jsonObj["output"], (string) jsonObj["time"]};
        }

        // Network functions
        private static string GetUrl(string url)
        {
            var request = WebRequest.Create(url);
            request.Timeout = 5000;
            var response = request.GetResponse().GetResponseStream();
            return response == null ? "" : new StreamReader(response).ReadToEnd();
        }

        private static string Hash(string str) =>
            string.Join("", new SHA1CryptoServiceProvider()
                .ComputeHash(Encoding.UTF8.GetBytes(str))
                .Select(b => b.ToString("x2")).ToArray()
                );
    }
}