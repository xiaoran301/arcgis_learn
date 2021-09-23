// Unity

using UnityEngine;

namespace ArcGISMapsSDK.UX.Security
{
	[System.Serializable]
	public class OAuthAuthenticationConfiguration
	{
		public string Name;
		public string ClientID;
		public string RedirectURI;
	}

	[System.Serializable]
	public class OAuthAuthenticationConfigurationMapping
	{
		public string ServiceURL;

		[SerializeReference]
		public OAuthAuthenticationConfiguration Configuration;
	}
}
