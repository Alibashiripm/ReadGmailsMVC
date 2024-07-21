using Google.Apis.Auth.OAuth2;

using Google.Apis.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Auth.OAuth2.Mvc;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Mvc;
using System.Text;
using static Google.Apis.Requests.BatchRequest;
using System.Collections.Concurrent;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System.IO;
using GmaiApiMVC.Models;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace GmaiApiMVC
{
    public class GoogleGmailApiHelper
    {
        private static HttpClient _httpClient;

        public static HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                    _httpClient.Timeout = TimeSpan.FromMinutes(5); // Set the timeout to 5 minutes
                }
                return _httpClient;
            }
        }
        private const string AppName = " ";
        public static UserCredential Credential { get; set; }
        public async static Task<List<MessageWithDetails>> GetInbox(RequestViewModel gmailModel)
        {
            var messagesWithDetails = new List<MessageWithDetails>();
			string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "info.json");
			string jsonContent = System.IO.File.ReadAllText(jsonFilePath);
			var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonContent);

			List<string> ValidSener = ((JArray)jsonObject.ValidSener).ToObject<List<string>>();

			try
			{
                var service = new GmailService(new BaseClientService.Initializer
                {
                    ApplicationName = AppName,
                    HttpClientInitializer = Credential
                });
                string nextPageToken = null;
                int remainingMessagesToFetch = 2000;
               

                do
                {
                    var listRequest = service.Users.Messages.List("me");
                    listRequest.MaxResults = 2000;
                    listRequest.PageToken = nextPageToken;
                    var response = await listRequest.ExecuteAsync();

                    var messages = response.Messages;
                    int i = 0;
                    var messageTasks = messages.Select(async message =>
                    {
                        try
                        {
                           
							// Define the message request.
							UsersResource.MessagesResource.GetRequest messageRequest = service.Users.Messages.Get("me", message.Id);
							messageRequest.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
							var messageDetails = await messageRequest.ExecuteAsync();
							var sender = messageDetails.Payload.Headers.FirstOrDefault(h => h.Name == "From")?.Value;
							string pattern = @"<(.*?)>";
							Match match = Regex.Match(sender, pattern);
							if (match.Success)
							{
							 
								sender = $"<{match.Groups[1].Value}>";

							}

							var details = new MessageWithDetails
                            {
                                Subject = messageDetails.Payload.Headers.FirstOrDefault(h => h.Name == "Subject")?.Value,
                                Body = GetMessageBody(messageDetails.Payload),
                                Sender= sender
							};
                            i++;
                            if (i > 100) {
                                await Task.Delay(TimeSpan.FromSeconds(3));
                                i =0;
                            }
                            messagesWithDetails.Add(details);
                        }
                        catch (Exception ex)
                        {
                          
                        }
                    });
                    await Task.WhenAll(messageTasks);
                    nextPageToken = response.NextPageToken;
                    remainingMessagesToFetch -= messages.Count();
                    
                } while (nextPageToken != null && remainingMessagesToFetch > 0);


		
				return messagesWithDetails.Where(w =>
					(w.Subject.Contains(gmailModel.Subject) || w.Subject == gmailModel.Subject) &&
					(ValidSener.Contains(w.Sender))
				).ToList();
			}
            catch (Exception ex)
            {
				return messagesWithDetails.Where(w =>
								(w.Subject.Contains(gmailModel.Subject) || w.Subject == gmailModel.Subject) &&
								(ValidSener.Contains(w.Sender))
							).ToList();
			}
		}
    
        public class MessageWithDetails
        {
            public string Subject { get; set; }
            public string Body { get; set; }
            public string Sender { get; set; }
        }
        private static string GetMessageBody(MessagePart payload)
        {
            if (payload.Body.Data != null)
            {
                return DecodeBase64String(payload.Body.Data);
            }
            else if (payload.Parts != null)
            {
                foreach (var part in payload.Parts)
                {
                    var body = GetMessageBody(part);
                    if (!string.IsNullOrEmpty(body))
                    {
                        return body;
                    }
                }
            }
            return null;
        }

        private static string DecodeBase64String(string base64String)
        {
            try
            {
                int padChars = (base64String.Length % 4) == 0 ? 0 : (4 - (base64String.Length % 4));
                StringBuilder result = new StringBuilder(base64String, base64String.Length + padChars);
                result.Append(String.Empty.PadRight(padChars, '='));
                result.Replace('-', '+');
                result.Replace('_', '/');
                byte[] data = Convert.FromBase64String(result.ToString());
                return Encoding.UTF8.GetString(data);

            }
            catch (Exception ex)
            {

                 return ex.Message;  
            }
        
        }
    }
}