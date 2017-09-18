using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NADAAPI.Repository
{
    public class ImpexiumRepository : IImpexiumRepository
    {
        private IHostingEnvironment _env;
        private ImpexiumProperties _prop;

        public ImpexiumRepository(IHostingEnvironment env, IOptions<ImpexiumProperties> prop)
        {
            _env = env;
            _prop = prop.Value;
        }
        public async Task<dynamic> CallFullUserProfile(string accessData, string IndividualID,bool details)
        {
            try
            {
                var p = accessData.Split('|');

                var appToken = p[0]; // "49a73e0d-f06c-42df-aa9e-8ec2163bd62d";//;accessData.appToken;
                var userToken = p[1]; // accessData.ssoToken; //SSO token
                var stopwatch = new Stopwatch();
                var getIndFindBySsoTokenMethod = "/Individuals/Profile/" + IndividualID + "/1?loadPrimaryOrgDetails=true&loadRelationships=" + details;

                var headerDictionary = new Dictionary<string, string>
                                           {
                                               { "AppToken", appToken },
                                               { "UserToken", userToken }
                                           };
                var baseUri = p[2]; // accessData.uri.ToString();
                var client = new RestClientNew(
                    baseUri + getIndFindBySsoTokenMethod,
                    HttpVerb.GET,
                    null,
                    headerDictionary);
                var json = await client.MakeRequest();


                if (!(json.Contains(" 404 ")))
                {
                    dynamic result = JsonConvert.DeserializeObject(json);
                    return result;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw ex;
            }
        }

        public async Task<dynamic> CallOrgProfile(string accessData, string OrgID)
        {
            try
            {
                var p = accessData.Split('|');
                var appToken = p[0]; // accessData.appToken;
                var userToken = p[1]; // accessData.ssoToken; //SSO token

                var FindBySsoTokenMethod = "/Organizations/Profile/" + OrgID + "/1";
                var getIndFindBySsoTokenMethod = FindBySsoTokenMethod;
                var headerDictionary = new Dictionary<string, string>
                                           {
                                               { "AppToken", appToken },
                                               { "UserToken", userToken }
                                           };
                var baseUri = p[2]; // accessData.uri.ToString();
                var client = new RestClientNew(
                    baseUri + getIndFindBySsoTokenMethod,
                    HttpVerb.GET,
                    null,
                    headerDictionary);
                var json = await client.MakeRequest();
                if (!(json.Contains("(404)")))
                {
                    dynamic result = JsonConvert.DeserializeObject(json);
                    return result;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw ex;
            }
        }

        public async Task<dynamic> CallOrgRelation(string accessData, string OrgID, string relName, bool details)
        {
            try
            {
                var FindByOrganizationRelationship =
                     string.Format("/Organizations/{0}/Relationships?RelationshipName={1}&includeDetails={2}", OrgID, relName, details);
                var getIndFindBySsoTokenMethod = FindByOrganizationRelationship;
                var p = accessData.Split('|');
                var appToken = p[0]; // accessData.appToken;
                var userToken = p[1]; // accessData.ssoToken; //SSO token

                var headerDictionary = new Dictionary<string, string>
                                           {
                                               { "AppToken", appToken },
                                               { "UserToken", userToken }
                                           };

                var baseUri = p[2]; // accessData.uri.ToString();
                var client = new RestClientNew(
                    baseUri + getIndFindBySsoTokenMethod,
                    HttpVerb.GET,
                    null,
                    headerDictionary);
                var json = await client.MakeRequest();
                if (!(json.Contains("(404)")))
                {
                    dynamic result = JsonConvert.DeserializeObject(json);
                    return result;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw ex;
            }
        }

       
        public Dictionary<string, object> GetDictionary(object input)
        {
            return input as Dictionary<string, object>;
        }

        public async Task<string> GetImpexiumAccessToken()
        {
            try
            {

                {
                    // if (HttpContext.Current.Session["local"] == null)
                    // values from api
                    var apiEndPoint = string.Empty;
                    var baseUri = string.Empty;
                    var accessToken = string.Empty;
                    var appToken = string.Empty;
                    var userToken = string.Empty;
                    var userString = string.Empty;

                    // Step 1 : Get ApiEndPoint and AccessToken
                    // POST api/v1/WebApiUrl

                    var client = new RestClientNew(
                        _prop.AccessEndPoint,
                        HttpVerb.POST,
                        "{\"AppName\":\"" + _prop.AppId + "\",\"AppKey\":\"" + _prop.AppKey + "\"}");
                    var json = await client.MakeRequest();
                    dynamic result = JsonConvert.DeserializeObject(json);
                    apiEndPoint = result.uri;
                    accessToken = result.accessToken;

                    // Step 2: Get AppToken or UserToken or Both
                    // POST api/v1/Signup/Authenticate
                    var headerDictionary = new Dictionary<string, string> { { "AccessToken", accessToken } };

                    client = new RestClientNew(
                        apiEndPoint,
                        HttpVerb.POST,
                        "{\"AppId\":\"" + _prop.AppId + "\",\"AppPassword\":\"" + _prop.AppKey
                        + "\",\"AppUserEmail\":\"" + _prop.ApiAccessEmail + "\",\"AppUserPassword\":\"" + _prop.ApiAccessPassword + "\"}",
                        headerDictionary);
                    json = await client.MakeRequest();
                    if (json != "ProtocolError")
                    {
                        result = JsonConvert.DeserializeObject(json);
                        appToken = result.appToken;
                        baseUri = result.uri;
                        userToken = result.ssoToken; // SSO token

                        // add to local storage 
                        var local = appToken + "|" + userToken + "|" + baseUri + "|" + accessToken + "|" + apiEndPoint;
                        this.WriteToFile(local);
                        return local;
                    }

                    return "Api Error";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetImpexiumCommitteemembers(string groupCode, bool isManager)
        {
            try
            {
                var rtnToken = await ValidateToken();
                var indTokenValues = rtnToken.Split('|');
                if (isManager)
                {
                    groupCode = groupCode + "_SUB";
                }
                var getGroupMembers = string.Format(
                           "/Committees/{0}/Members/1",
                           groupCode);

                var appToken = indTokenValues[0];
                var userToken = indTokenValues[1];

                var headerDictionary = new Dictionary<string, string>
                               {
                                 { "AppToken", appToken },
                                 { "UserToken", userToken }
                                };

                var baseUri = indTokenValues[2]; // accessData.uri.ToString();
                var client = new RestClientNew(
                    baseUri + getGroupMembers,
                    HttpVerb.GET,
                    null,
                    headerDictionary);
                var json = await client.MakeRequest();

                if (json.ToString() != "ProtocolError")
                {
                    return json.ToString();
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw ex;
            }
        }

        public async Task<string> GetImpexiumUser(string IndividualID)
        {
            try
            {
                var rtnToken = await ValidateToken();
                var indTokenValues = rtnToken.Split('|');
                var appToken = indTokenValues[0];
                var userToken = indTokenValues[1];
                var baseUri = indTokenValues[2];
                var getIndFindBySsoTokenMethod = "/Individuals/Profile/" + IndividualID + "/1";

                var headerDictionary = new Dictionary<string, string>
                                               {
                                                   { "AppToken", appToken },
                                                   { "UserToken", userToken }
                                               };

                var client = new RestClientNew(
                    baseUri + getIndFindBySsoTokenMethod,
                    HttpVerb.GET,
                    null,
                    headerDictionary);
                var json = await client.MakeRequest();

                if (json.ToString() != "ProtocolError")
                {
                    return json.ToString();
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw ex;
            }
        }

        public List<object> GetList(object input)
        {
            return input as List<object>;
        }

        public Dictionary<string, object> ParseJsonToDictionary(string input)
        {
            // This utility method converts a JSON string into a dictionary of names and values for easy access
            if (input != null)
            {
                var initial = JsonConvert.DeserializeObject<Dictionary<string, object>>(input);
                return initial.ToDictionary(
                    d => d.Key,
                    d =>
                    d.Value is JArray
                        ? ParseJsonToList(d.Value.ToString())
                        : (d.Value is JObject ? ParseJsonToDictionary(d.Value.ToString()) : d.Value));
            }

            return null;
        }

        public List<object> ParseJsonToList(string input)
        {
            // This utility method converts a JSON string into a dictionary of names and values for easy access
            if (input != null)
            {
                var initial = JsonConvert.DeserializeObject<List<object>>(input);
                return
                    initial.Select(
                        d =>
                        d is JArray
                            ? ParseJsonToList(d.ToString())
                            : (d is JObject ? ParseJsonToDictionary(d.ToString()) : d)).ToList();
            }

            return null;
        }

        public string ReadfileAndReturnString()
        {
            string text;
            var path = "/Token/token.txt";
            var fileStream = new FileStream(_env.WebRootPath + path, FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                text = streamReader.ReadToEnd();
                text = text.TrimEnd('\r', '\n');
            }

            if (text == string.Empty || text == null)
            {
                var x = this.GetImpexiumAccessToken();
            }

            return text;
        }

        public async Task<string> ValidateToken()
        {
            try
            {
                var stopwatch = new Stopwatch();

                var Token = this.ReadfileAndReturnString();
                var p = Token.Split('|');

                var ads = "/Ads/Mobile/All/1";
                var getIndFindBySsoTokenMethod = ads;

                var appToken = p[0]; // "49a73e0d-f06c-42df-aa9e-8ec2163bd62d";//;accessData.appToken;
                var userToken = p[1]; // accessData.ssoToken; //SSO token

                var headerDictionary = new Dictionary<string, string>
                                           {
                                               { "AppToken", appToken },
                                               { "UserToken", userToken }
                                           };

                var baseUri = p[2]; // accessData.uri.ToString();
                var client = new RestClientNew(
                    baseUri + getIndFindBySsoTokenMethod,
                    HttpVerb.GET,
                    null,
                    headerDictionary);
                var json = await client.MakeRequest();

                // if contains 401 in the error means the Token is Expired
                if (json.Contains("(401)") || json.Contains("(404)"))
                {
                    await this.GetImpexiumAccessToken();
                    Token = this.ReadfileAndReturnString();
                }


                return Token;
            }
            catch (Exception ex)
            {
                await GetImpexiumAccessToken();
                return ex.ToString();
            }
        }

        public void WriteToFile(string DataTobewritten)
        {
            try
            {
                var path = "/Token/token.txt";
                if (!File.Exists(_env.WebRootPath + path))
                {
                    File.Create(_env.WebRootPath + path);
                }
                else
                {
                    using (var fs = new FileStream(_env.WebRootPath + path, FileMode.Truncate))
                    {
                    }
                }

                using (var w = File.AppendText(_env.WebRootPath + path))
                {
                    var err = DataTobewritten;
                    w.WriteLine(err);
                    w.Flush();
                }
            }
            catch (Exception )
            {
                //Response.Write(ex.Message, "When updating ErrorMessage");
            }
        }

        #region  Log Any Errors

        public void WriteError(string errorMessage, string methodName, string errorType = "Ektron", string additionalParameters = "", bool sendEmail = true)
        {
            try
            {

                string path = "/Error/" + DateTime.Today.ToString("dd-MM-yyyy") + ".txt";
                if ((!File.Exists(_env.WebRootPath + path)))
                {
                    File.Create(_env.WebRootPath + path);
                }
                using (StreamWriter w = File.AppendText(_env.WebRootPath + path))
                {
                    w.WriteLine("***********************************************************************");
                    // w.WriteLine(Constants.vbCrLf + "Log Entry : ");
                    w.WriteLine("{0}", DateTime.Now.ToString());
                    string err = "<b>Error on page :</b>" + _env.WebRootPath.ToString()
                                 + ".<br/> <b>Error Message:</b><br/>" + errorMessage;
                    w.WriteLine(err);
                    w.WriteLine("__________________________");
                    w.WriteLine(methodName);
                    w.WriteLine("__________________________");
                    w.WriteLine("***********************************************************************");
                    if (sendEmail)
                    {
                        string email = _prop.ApiAccessEmail;
                        string subject = _env.WebRootPath.ToString() + " - " + errorType + " - " + methodName;
                        string messge = "<b>Domain of Origin :</b>" + _env.WebRootPath.ToString()
                                    + "<br/>" + "<b>Additional Info :</b><br/>" + additionalParameters
                                     + "<br/>" + err;
                        SendEmailAlert(email, subject, messge);
                    }
                    w.Flush();
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //Response.Write(ex.Message, "When updating ErrorMessage");
            }

        }

        public void SendEmailAlert(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("NoReply_NADA", "noreply@nada.org"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Cc.Add(new MailboxAddress("skadire@nada.org"));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = message };

            using (var client = new SmtpClient())
            {
                client.LocalDomain = "NADA.org";
                client.Connect(_prop.smtpClient, 25, SecureSocketOptions.None);
                try
                {
                    client.Send(emailMessage);
                    client.Disconnect(true);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        #endregion


    }
}
