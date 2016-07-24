using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BFClient_CSharp.Util
{
    static class SessionMgr
    {
        public static string Host = "http://localhost:8081";
        public static string Username = "";
        static string _sessionId = "";

        // User login & logout
        public static void Login(string username, string password)
        {
            var serverResp = GetUrl($"{Host}/user/login?username={username}&pwdhash={Hash(password)}");
            var jsonObj = JObject.Parse(serverResp);
            if ((int) jsonObj["result"] < 0)
                throw new Exception((string) jsonObj["errmsg"]);
            else
            {
                _sessionId = (string) jsonObj["sessid"];
                Username = username;
            }
        }

        public static void SaveLoginInfo(string username, string password)
        {
            Properties.Settings.Default.host = Host;
            Properties.Settings.Default.username = username;
            Properties.Settings.Default.password = password;
            Properties.Settings.Default.Save();
        }

        public static bool TryAutoLogin()
        {
            Host = Properties.Settings.Default.host;
            var username = Properties.Settings.Default.username;
            var password = Properties.Settings.Default.password;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty((password))) return false;
            try
            {
                Login(username, password);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void Logout()
        {
            Username = "";
            _sessionId = "";
        }

        public static void ChangePassword(string oldPassword, string newPassword)
        {
            var serverResp = GetUrl($"{Host}/user/changepassword?sessid={_sessionId}&oldpwdhash={Hash(oldPassword)}&newpwdhash={Hash(newPassword)}");
            var jsonObj = JObject.Parse(serverResp);
            if ((int) jsonObj["result"] < 0)
                throw new Exception((string)jsonObj["errmsg"]);
        }

        // File I/O
        public static string FileList() => GetUrl($"{Host}/io/list?sessid={_sessionId}");

        public static string FileContent(string filename, string version)
        {
            var serverResp = GetUrl($"{Host}/io/open?sessid={_sessionId}&filename={filename}&version={version}");
            var jsonObj = JObject.Parse(serverResp);
            if ((int) jsonObj["result"] < 0)
                throw new Exception((string)jsonObj["errmsg"]);
            else
                return (string) jsonObj["code"];
        }

        public static string SaveFile(string code, string filename)
        {
            var encCode = HttpUtility.UrlEncode(HttpUtility.UrlEncode(code));
            var serverResp = GetUrl($"{Host}/io/save?sessid={_sessionId}&code={encCode}&filename={filename}");
            var jsonObj = JObject.Parse(serverResp);
            if ((int) jsonObj["result"] < 0)
                throw new Exception((string) jsonObj["errmsg"]);
            else
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
            else
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
