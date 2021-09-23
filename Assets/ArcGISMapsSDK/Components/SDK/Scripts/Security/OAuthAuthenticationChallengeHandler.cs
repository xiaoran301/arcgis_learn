using System;
using System.Threading.Tasks;

namespace Esri.ArcGISMapsSDK.Security
{
	public abstract class OAuthAuthenticationChallengeHandler : IDisposable
	{
		public abstract void Dispose();

		public abstract Task<string> HandleChallenge(string authorizeURI);
	};
}
