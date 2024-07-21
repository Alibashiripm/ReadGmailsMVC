using GmaiApiMVC.Models;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using static GmaiApiMVC.GoogleGmailApiHelper;
using System.Net;
using static Google.Apis.Gmail.v1.UsersResource;
using System.Net.Http;

namespace GmaiApiMVC.Controllers
{
    public class GmailController : Controller
    {
         
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]

    
        public async Task<string> Index(RequestViewModel request, CancellationToken cancellationToken)
        {
			System.Web.HttpContext.Current.Server.ScriptTimeout = 3000;

			string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "info.json");
			string jsonContent = System.IO.File.ReadAllText(jsonFilePath);
			var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonContent);
			string url = jsonObject.url;
			// Create an instance of HttpClient
			using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url) ;
                

                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");

                var response = await client.GetAsync($"/home/GetGoogleContacts?key={request.Key}&Subject={request.Subject}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // Process response content here
                    return responseContent;
                }
                else
                {
                    // Handle non-success status codes here
                    return "error";
                }
            }
        }
    }
}
 