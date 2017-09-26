using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Security;
using System.Text;
using System.Web;

namespace SocialcastBot
{
    /// <summary>
    /// Socialcast bot API endpoint
    /// </summary>
    public class Api
    {
        private readonly Uri _uri;
        private readonly CredentialCache _credentialCache;
        private readonly CookieContainer _cookieContainer;
        private readonly string _userAgent;
        private readonly ApiResponseFormat _responseFormat;
        private WebProxy _webProxy;

        /// <summary>
        /// Construct the API endpoint
        /// </summary>
        /// <param name="uri">Location of your community, e.g. https://demo.socialcast.com</param>
        /// <param name="userAgent">Name of your bot</param>
        /// <param name="username">Username or e-mail address</param>
        /// <param name="password">Password</param>
        /// <param name="responseFormat">Format of the response</param>
        public Api(Uri uri, string userAgent, string username, SecureString password, ApiResponseFormat responseFormat)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls12;

            _credentialCache = new CredentialCache();
            _cookieContainer = new CookieContainer();
            _uri = uri;
            _userAgent = userAgent;
            _responseFormat = responseFormat;

            _credentialCache.Add(_uri, "Basic", new NetworkCredential(username, password));
        }

        /// <summary>
        /// Set HTTP proxy settings for the API
        /// </summary>
        /// <param name="webProxy">HTTP proxy settings</param>
        public void SetProxy(WebProxy webProxy)
        {
            _webProxy = webProxy;
        }

        /// <summary>
        /// Determine if the user has correct credentials
        /// https://socialcast.github.io/socialcast/apidoc/authentication.html
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>Response of the API endpoint</returns>
        public ApiResponse Authenticate(string email, string password)
        {
            var uri = "/api/authentication";

            var postData = new Dictionary<string, string>
            {
                {"email", email},
                {"password", password}
            };

            return DoRequest(uri, WebRequestMethods.Http.Post, null, postData);
        }

        /// <summary>
        /// Get a list of users for the current community
        /// https://socialcast.github.io/socialcast/apidoc/users/index.html
        /// </summary>
        /// <param name="state">User's state, must be one of: inactive, terminated, all, active</param>
        /// <param name="page">Page number to view</param>
        /// <param name="perPage">Number of users to be returned per page</param>
        /// <returns>Response of the API endpoint</returns>
        public ApiResponse GetUsers(string state = "active", int page = 1, int perPage = 20)
        {
            var uri = "/api/users";

            var getData = new Dictionary<string, string>
            {
                {"state", state},
                {"page", page.ToString()},
                {"per_page", perPage.ToString()}
            };

            return DoRequest(uri, WebRequestMethods.Http.Get, getData, null);
        }

        /// <summary>
        /// Show information for a user
        /// https://socialcast.github.io/socialcast/apidoc/users/show.html
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>Response of the API endpoint</returns>
        public ApiResponse ShowUser(string userId)
        {
            var uri = "/api/users/:user_id";

            uri = uri.Replace(":user_id", userId);

            return DoRequest(uri, WebRequestMethods.Http.Get, null, null);
        }

        private ApiResponse DoRequest(string resource, string requestMethod, Dictionary<string, string> getData, Dictionary<string, string> postData)
        {
            ApiResponse apiResponse = null;

            var parameters = string.Empty;
            if (getData != null)
            {
                var queryString = HttpUtility.ParseQueryString(string.Empty);
                foreach (var item in getData)
                {
                    queryString.Add(item.Key, item.Value);
                }
                parameters = string.Concat("?", queryString.ToString());
            }

            var request = WebRequest.CreateHttp(new Uri(_uri, string.Concat(resource, ".", _responseFormat.ToString().ToLowerInvariant(), parameters)));
            request.Credentials = _credentialCache;
            request.CookieContainer = _cookieContainer;
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            request.KeepAlive = false;
            request.Proxy = _webProxy;
            request.UserAgent = _userAgent;
            request.Timeout = 30 * 1000;
            request.ReadWriteTimeout = 30 * 1000;
            request.ContinueTimeout = 1 * 1000;
            request.Method = requestMethod;

            if (postData != null)
            {
                var queryString = HttpUtility.ParseQueryString(string.Empty);
                foreach (var item in postData)
                {
                    queryString.Add(item.Key, item.Value);
                }
                var postBytes = Encoding.UTF8.GetBytes(queryString.ToString());
                request.ContentLength = postBytes.Length;
                request.ContentType = "application/x-www-form-urlencoded";

                var requestStream = request.GetRequestStream();
                requestStream.Write(postBytes, 0, postBytes.Length);
            }

            try
            {
                var response = request.GetResponse();

                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    var data = streamReader.ReadToEnd();
                    streamReader.Close();
                    apiResponse = new ApiResponse(true, _responseFormat, data);
                }

                response.Close();
            }
            catch (WebException ex)
            {
                apiResponse = new ApiResponse(false, null, ex.Message);
            }

            return apiResponse;
        }
    }
}
