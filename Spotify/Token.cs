using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Utils;


namespace Spotify
{
    class TokenCache
    {
        #region Fields & Properties

        public string PathToCache { get; private set; }

        private Dictionary<string, Dictionary<string, object>> loadedCache;

        #endregion

        #region Constructors

        public TokenCache(string path)
        {
            PathToCache = $"{path}/token_cache.json";

            loadedCache = GetJsonFromCache();
        }

        #endregion

        #region Helper Methods

        private string MakeKey(string userEmail, Scope scope)
        {
            return $"{userEmail}:{scope.ToString()}";
        }

        #endregion

        #region Public Interface
        public Token GetToken(string userEmail, Scope scope)
        {
            string key = MakeKey(userEmail, scope);

            if (!ContainsKey(userEmail, scope))
                throw new KeyNotFoundException($"Key '{key}' was not found.");

            Console.WriteLine(loadedCache[key]);

            return Token.FromDict((Dictionary<string, object>)loadedCache[key]);
        }

        public void PutToken(Token token)
        {
            string key = MakeKey(token.UserEmail, token.Scope);

            loadedCache[key] = token.ToDict();

            SaveJsonToCache(loadedCache);
        }

        public bool ContainsKey(string userEmail, Scope scope)
        {
            return loadedCache.ContainsKey(MakeKey(userEmail, scope));
        }

        #endregion

        #region Load & Save Methods

        private Dictionary<string, Dictionary<string, object>> GetJsonFromCache()
        {
            if (File.Exists(PathToCache))
            {
                using (StreamReader streamReader = new StreamReader(PathToCache))
                {
                    string jsonString = streamReader.ReadToEnd();

                    return JsonConvert.
                        DeserializeObject<Dictionary<string, Dictionary<string, object>>>(jsonString);
                }
            }
            else return new Dictionary<string, Dictionary<string, object>>();
        }

        private void SaveJsonToCache(Dictionary<string, Dictionary<string, object>> jsonDict)
        {
            string jsonString = JsonConvert.SerializeObject(jsonDict, Formatting.Indented);

            using (StreamWriter streamWriter = new StreamWriter(PathToCache))
            {
                streamWriter.Write(jsonString);
            }
        }

        #endregion
    }


    class Token
    {
        #region Fields & Properties

        public string UserEmail { get; private set; }
        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public Scope Scope { get; private set; }

        private int expiresIn;
        private int timeObtained;

        #endregion

        #region Constructors

        public Token(string userEmail, string accessToken, string refreshToken,
            string scope, int expiresIn, int timeObtained)
        {
            UserEmail = userEmail;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            Scope = Scope.FromString(scope);

            this.expiresIn = expiresIn;
            this.timeObtained = timeObtained;

            if (this.timeObtained == 0)
                this.timeObtained = (int) DateTime.Now.GetUnixEpoch();
        }

        #endregion

        #region Helper Method

        public int GetTimeOfExpiry()
        {
            return (int) (expiresIn + timeObtained);
        }

        public bool IsExpired()
        {
            return GetTimeOfExpiry() - DateTime.Now.GetUnixEpoch() < 0.0;
        }

        public override string ToString()
        {
            return $"Token({Scope}, IsExpired={IsExpired()})";
        }

        #endregion

        #region Parsing Methods

        public static Token FromDict(Dictionary<string, object> jsonDict)
        {
            string userEmail = (string) jsonDict["user_email"];
            string accessToken = (string) jsonDict["access_token"];
            string refreshToken = jsonDict.ContainsKey("refresh_token") ? (string) jsonDict["refresh_token"] : "";
            string scope = (string) jsonDict["scope"];
            int expiresIn = (int)(long) jsonDict["expires_in"];
            int timeObtained = jsonDict.ContainsKey("time_obtained") ? (int)(long) jsonDict["time_obtained"] : 0;

            return new Token(userEmail, accessToken, refreshToken, scope, expiresIn, timeObtained);
        }

        public Dictionary<string, object> ToDict()
        {
            var returnDict = new Dictionary<string, object>()
            {
                { "user_email", UserEmail },
                { "access_token", AccessToken },
                { "refresh_token", RefreshToken},
                { "scope", Scope.ToString()},
                { "expires_in" , expiresIn},
                { "time_obtained", timeObtained}
            };

            return returnDict;
        }

        #endregion
    }
}