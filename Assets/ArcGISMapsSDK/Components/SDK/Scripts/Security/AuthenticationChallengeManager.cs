using UnityEngine;

namespace Esri.ArcGISMapsSDK.Security
{
	public class AuthenticationChallengeManager
	{
		private static OAuthAuthenticationChallengeHandler oauthChallengeHandler;

		public static OAuthAuthenticationChallengeHandler OAuthChallengeHandler
		{
			get
			{
				return oauthChallengeHandler;
			}
			set
			{
				oauthChallengeHandler = value;

				GameEngine.Security.ArcGISAuthenticationManager.OAuthAuthenticationChallengeIssued = delegate (GameEngine.Security.ArcGISOAuthAuthenticationChallenge authChallenge)
				{
					ArcGISMapsSDK.Core.Utils.MainThreadScheduler.Instance().Schedule(() =>
					{
						oauthChallengeHandler.HandleChallenge(authChallenge.AuthorizeURI).ContinueWith(authorizationCodeTask =>
						{
							if (authorizationCodeTask.IsFaulted)
							{
								Debug.LogError(authorizationCodeTask.Exception.Message);

								authChallenge.Cancel();
							}
							else if (authorizationCodeTask.IsCanceled)
							{
								authChallenge.Cancel();
							}
							else
							{
								var authorizationCode = authorizationCodeTask.Result;

								if (authorizationCode != null)
								{
									authChallenge.Respond(authorizationCode);
								}
								else
								{
									authChallenge.Cancel();
								}
							}
						});
					});
				};
			}
		}
	}
}
