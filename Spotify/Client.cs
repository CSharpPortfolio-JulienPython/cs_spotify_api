using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Utils;


namespace Spotify
{
    #region Exceptions

    class HttpException : Exception
    {
        public HttpException(HttpStatusCode statusCode, string reasonPhrase)
            : base($"{(int)statusCode}: {reasonPhrase}") { }
    }

    class IncorrectStateException : Exception
    {
        public IncorrectStateException(int expectedState, int receivedState)
            : base($"Incorrect state. Expected {expectedState}, but received {receivedState}.") { }
    }

    #endregion

    class Client
    {
        #region Constants

        private const string clientId = ""; // Fill in your client id.
        private const string clientSecret = ""; // Fill in your client secret.
        private const string redirectUri = ""; // Fill in your redirect uri.

        private const string urlForAuth = "https://accounts.spotify.com/authorize";
        private const string urlForToken = "https://accounts.spotify.com/api/token";
        private const string urlForApi = "https://api.spotify.com/v1";

        #endregion

        #region Fields & Properties

        private int state;
        private bool showDialog;

        private HttpClient httpClient;
        private TokenCache tokenCache;

        public string UserEmail { get; private set; }
        public string PathToTokenCache { get; private set; }
        public Scope Scope { get; private set; }

        private Token _token;
        public Token Token
        {
            get
            {
                if (_token != null && _token.IsExpired())
                    _token = RefreshToken(_token);
                
                return _token;
            }
            private set { _token = value; }
        }


        #endregion

        #region Constructors

        public Client(string userEmail = "", string pathToTokenCache = ".",
            Scope scope = null, int state = 0, bool showDialog = false)
        {
            UserEmail = userEmail;
            PathToTokenCache = pathToTokenCache;
            Scope = scope ?? Scope.PlaylistReadPrivate;

            this.state = state;
            this.showDialog = showDialog;

            tokenCache = new TokenCache(pathToTokenCache);
            httpClient = new HttpClient();

            Token = GetToken();
        }

        #endregion

        #region Helper Methods

        private AuthenticationHeaderValue GetBasicAuthHeader()
        {
            string encHeader = System.Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            return new AuthenticationHeaderValue("Basic", encHeader);
        }

        private AuthenticationHeaderValue GetBearerAuthHeader()
        {
            return new AuthenticationHeaderValue("Bearer", "");
        }

        #endregion

        #region Http Methods

        private object GetJsonFromHttpResponse(HttpResponseMessage httpResponse)
        {
            if (!httpResponse.IsSuccessStatusCode)
                throw new HttpException(httpResponse.StatusCode, httpResponse.ReasonPhrase);

            using (HttpContent content = httpResponse.Content)
            {
                string jsonString = content.ReadAsStringAsync().Result;
                
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            }
        }

        private object HttpGet(string url, Dictionary<string, object> rawData, 
            AuthenticationHeaderValue authHeader)
        {
            string encodedData = HelperMethods.EncodeStringToHttpParameters(rawData);

            httpClient.DefaultRequestHeaders.Authorization = authHeader;

            using (HttpResponseMessage httpResponse = httpClient.GetAsync($"{url}?{encodedData}").Result)
            {
                return GetJsonFromHttpResponse(httpResponse);
            }
        }

        private object HttpPost(string url, Dictionary<string, object> rawData, 
            AuthenticationHeaderValue authHeader)
        {
            HttpContent encodedData = new FormUrlEncodedContent(
                rawData.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()));

            httpClient.DefaultRequestHeaders.Authorization = authHeader;

            using (HttpResponseMessage httpResponse = httpClient.PostAsync(url, encodedData).Result) 
            {
                return GetJsonFromHttpResponse(httpResponse);
            }
        }

        #endregion

        #region Autorization Methods

        private Token GetToken()
        {
            Token token = null;

            if (UserEmail != "") // If no user, token will not be in cache.
            {
                if (tokenCache.ContainsKey(UserEmail, Scope))
                    token = tokenCache.GetToken(UserEmail, Scope);

                if (token != null && token.IsExpired())
                    token = RefreshToken(token);
            }

            if (token == null) // If token not in cache.
            {
                if (UserEmail != "")
                    token = GetTokenFromWebWithUser();
                else    
                    token = GetTokenFromWebWithoutUser();

                tokenCache.PutToken(token);
            }

            return token;
        }

        private Token GetTokenFromHttpRequest(Dictionary<string, object> data)
        {
            var response = (Dictionary<string, object>) HttpPost(urlForToken, data, GetBasicAuthHeader());

            response["user_email"] = UserEmail;

            return Token.FromDict(response);
        }

        private Token GetTokenFromWebWithUser()
        {
            var data = new Dictionary<string, object>()
            {
                { "client_id",      clientId },
                { "response_type",  "code" },
                { "redirect_uri",   redirectUri },
                { "state",          state.ToString() },
                { "scope",          Scope.ToString() },
                { "show_dialog",    showDialog.ToString().ToLower() }
            };

            string authUrl = HelperMethods.EncodeToUrl(urlForAuth, data);

            Console.WriteLine($"1. Paste following url in browser:\n\n{authUrl}\n");
            Console.WriteLine($"2. Follow instructions and paste resulting url here:\n");

            string returnUrl = Console.ReadLine();

            Dictionary<string, object> parameters = HelperMethods.ExtractParametersFromUrl(returnUrl);

            int receivedState = int.Parse((string) parameters["state"]);

            if (receivedState != this.state)
                throw new IncorrectStateException(this.state, receivedState);

            data = new Dictionary<string, object>()
            {
                { "grant_type",     "authorization_code" },
                { "code",           (string) parameters["code"] },
                { "redirect_uri",   redirectUri}
            };

            return GetTokenFromHttpRequest(data);
        }

        private Token GetTokenFromWebWithoutUser()
        {
            var data = new Dictionary<string, object>()
            {
                {"grant_type", "client_credentials"}
            };

            return GetTokenFromHttpRequest(data);
        }

        private Token RefreshToken(Token token)
        {
            Token outToken;

            if (UserEmail != "")
            {
                var data = new Dictionary<string, object>()
                {
                    { "grant_type",     "refresh_token"},
                    { "refresh_token",  token.RefreshToken}
                };

                outToken = GetTokenFromHttpRequest(data);

                tokenCache.PutToken(outToken);
            }
            else
                outToken = GetTokenFromWebWithoutUser();

            return outToken;
        }
        
        #endregion
    }
}