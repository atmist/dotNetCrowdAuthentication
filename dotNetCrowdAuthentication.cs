using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace dotNetCrowdAuthentication.Service
{
    public class dotNetCrowdAuthentication
    {
        /// <summary>
        /// Instatiate the authentication service
        /// </summary>
        /// <param name="crowdUrl">Url of the hosted Crowd instance</param>
        /// <param name="applicationName">The application name set in Crowd</param>
        /// <param name="applicationPassword">The password for the respective application set in Crowd</param>
        public dotNetCrowdAuthentication(string crowdUrl, string applicationName, string applicationPassword)
        {
            CROWD_URL = crowdUrl;
            APPLICATION_NAME = applicationName;
            APPLICATION_PASSWORD = applicationPassword;
        }

        #region Global Variables

        private string APPLICATION_NAME = string.Empty;
        private string APPLICATION_PASSWORD = string.Empty;
        private string CROWD_URL = string.Empty;

        private string displayName = string.Empty;
        public string DisplayName
        {
            get
            {
                return displayName;
            }
            private set
            {
                displayName = string.Empty;
            }
        }

        private string email = string.Empty;
        public string Email
        {
            get
            {
                return email;
            }
            private set
            {
                email = string.Empty;
            }
        }

        private const string DISPLAY_NAME = "display-name";
        private const string EMAIL = "email";

        #endregion

        /// <summary>
        /// Authenticate against Crowd via REST API
        /// </summary>
        /// <param name="username">The username of the user that is set in Crowd</param>
        /// <param name="password">The password of the user that is set in Crowd</param>
        /// <returns>Returns TRUE if the user has provided correct credentials. False otherwise</returns>
        /// <remarks>Throws WebException if authentication fails</remarks>
        public bool Authenticate(string username, string password)
        {
            var request = (HttpWebRequest)WebRequest.Create(CROWD_URL + "/usermanagement/1/authentication?username=" + username);
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Method = "POST";
            request.Headers[HttpRequestHeader.Authorization] = string.Format("Basic " + Encode(APPLICATION_NAME, APPLICATION_PASSWORD));

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                var json = JsonConvert.SerializeObject(
                    new
                    {
                        value = password
                    });
                writer.Write(json);

            }
            try
            {
                var result = (HttpWebResponse)request.GetResponse();
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    using (var reader = new StreamReader(result.GetResponseStream()))
                    {
                        var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(reader.ReadToEnd());
                        displayName = json[DISPLAY_NAME].ToString();
                        email = json[EMAIL].ToString();
                    }
                    return true;
                }
            }
            catch (WebException)
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// Remove all the information pertaining to the currently logged in user
        /// </summary>
        public void DeleteInformation()
        {
            email = string.Empty;
            displayName = string.Empty;
        }

        #region Private Methods

        private static string Encode(string username, string password)
        {
            var auth = string.Join(":", username, password);
            return Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(auth));
        }

        #endregion
    }
}
