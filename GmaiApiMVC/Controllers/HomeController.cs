using Google.Apis.Auth.OAuth2.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System.IO;
using Google.Apis.Gmail.v1.Data;
using System.Net.Http;
using static GmaiApiMVC.GoogleGmailApiHelper;
using GmaiApiMVC.Models;
using Org.BouncyCastle.Asn1.Ocsp;

namespace GmaiApiMVC.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
         
            try
            {
                string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file.json");
                string jsonContent = System.IO.File.ReadAllText(jsonFilePath);
                var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                var flow = new AppFlowMetadata();
                UserCredential googlecredential = new UserCredential(flow.Flow, null,
                    new Google.Apis.Auth.OAuth2.Responses.TokenResponse()
                    {
                        AccessToken = jsonObject.Token.access_token,
                        TokenType = jsonObject.Token.token_type,
                        ExpiresInSeconds = jsonObject.Token.expires_in,
                        RefreshToken = jsonObject.Token.refresh_token,
                        Issued = jsonObject.Token.Issued,
                        IssuedUtc = jsonObject.Token.IssuedUtc,
                        Scope = "https://www.googleapis.com/auth/contacts https://www.googleapis.com/auth/contacts.other.readonly",
                        IdToken = null
                    }
                    );
                bool refreshed = await googlecredential.RefreshTokenAsync(cancellationToken);
                if (refreshed)
                {
                    ViewBag.IsLogin = true;
                    return View("Index");
                }
                else
                {
                    ViewBag.IsLogin = false;
                    return View("Index");
                }

            }
            catch
            {
                ViewBag.IsLogin = false;
                return View("Index");
            }
          
        }


        public async Task<ActionResult> AuthorizeGoogleContacts(CancellationToken cancellationToken)
		{
			System.Web.HttpContext.Current.Server.ScriptTimeout = 3000;

			try
			{
                string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file.json");
                string jsonContent = System.IO.File.ReadAllText(jsonFilePath);
                var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                var flow = new AppFlowMetadata();
                UserCredential googlecredential = new UserCredential(flow.Flow, null,
                    new Google.Apis.Auth.OAuth2.Responses.TokenResponse()
                    {
                        AccessToken = jsonObject.Token.access_token,
                        TokenType = jsonObject.Token.token_type,
                        ExpiresInSeconds = jsonObject.Token.expires_in,
                        RefreshToken = jsonObject.Token.refresh_token,
                        Issued = jsonObject.Token.Issued,
                        IssuedUtc = jsonObject.Token.IssuedUtc,
                        Scope = "https://www.googleapis.com/auth/contacts https://www.googleapis.com/auth/contacts.other.readonly",
                        IdToken = null
                    }
                    );
                bool refreshed = await googlecredential.RefreshTokenAsync(cancellationToken);
                if (refreshed)
                {
                    ViewBag.IsLogin = true;
                    return View("Index");
                }
                else
                {
                    var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata())
                    .AuthorizeAsync(cancellationToken);
                    return new RedirectResult(result.RedirectUri);
                }

            }
            catch
            {
                var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata())
                    .AuthorizeAsync(cancellationToken);
                return new RedirectResult(result.RedirectUri);
            }


        }
        [HttpGet]
        public async Task<string> GetGoogleContacts(string key,string subject, CancellationToken cancellationToken)
        {
            try
			{
				string _jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "info.json");
				string _jsonContent = System.IO.File.ReadAllText(_jsonFilePath);
				var _jsonObject = JsonConvert.DeserializeObject<dynamic>(_jsonContent);

				string _key = _jsonObject.Key;
				if (key == _key)
                {


                    string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file.json");
                    string jsonContent = System.IO.File.ReadAllText(jsonFilePath);
                    var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                    var flow = new AppFlowMetadata();
                    UserCredential googlecredential = new UserCredential(flow.Flow, null,
                        new Google.Apis.Auth.OAuth2.Responses.TokenResponse()
                        {
                            AccessToken = jsonObject.Token.access_token,
                            TokenType = jsonObject.Token.token_type,
                            ExpiresInSeconds = jsonObject.Token.expires_in,
                            RefreshToken = jsonObject.Token.refresh_token,
                            Issued = jsonObject.Token.Issued,
                            IssuedUtc = jsonObject.Token.IssuedUtc,
                            Scope = "https://www.googleapis.com/auth/contacts https://www.googleapis.com/auth/contacts.other.readonly",
                            IdToken = null
                        }
                        );
                    bool refreshed = await googlecredential.RefreshTokenAsync(cancellationToken);
                    string json = JsonConvert.SerializeObject(googlecredential);
                    System.IO.File.WriteAllText(jsonFilePath, json);
                    GoogleGmailApiHelper.Credential = googlecredential;
                    RequestViewModel gmailModel = new RequestViewModel()
                    {
                        Subject = subject,// Subject = requestModel.Subject,
                    };

                    Task<List<MessageWithDetails>> inboxMessagesTask = GoogleGmailApiHelper.GetInbox(gmailModel);
                    await inboxMessagesTask.ConfigureAwait(false);
                    List<MessageWithDetails> inboxMessages = inboxMessagesTask.Result;

                 var response = JsonConvert.SerializeObject(inboxMessages);
                    var deser = JsonConvert.DeserializeObject<List<MessageWithDetails>>(response);
                    return response;


                }
                else
                {
                    return null;

                }
            }
            catch (Exception ex)
            {

                throw;
            }
           
        }
    
    }
}