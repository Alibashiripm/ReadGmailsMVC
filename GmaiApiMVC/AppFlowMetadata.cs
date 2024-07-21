using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.IO;
using System;

public class AppFlowMetadata : FlowMetadata
{

	private static ClientSecrets InitializeClientSecrets()
	{
		string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "info.json");
		string jsonContent = System.IO.File.ReadAllText(jsonFilePath);
		var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonContent);

		string clientId = jsonObject.ClientId;
		string clientSecret = jsonObject.ClientSecret;

		return new ClientSecrets
		{
			ClientId = clientId,
			ClientSecret = clientSecret
		};
	}
	private static	ClientSecrets clientSecrets = InitializeClientSecrets();

	private static readonly IAuthorizationCodeFlow flow =
		new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
		{
			            ClientSecrets = clientSecrets,

			Scopes = new[] { GmailService.Scope.GmailReadonly }
		});

	public override IAuthorizationCodeFlow Flow
	{
		get { return flow; }
	}

	public override string GetUserId(Controller controller)
	{
		return "";
	}
}
