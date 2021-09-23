using Esri.ArcGISMapsSDK.Security;
using UnityEngine;

public class OAuthChallengeHandlersInitializer : MonoBehaviour
{
	private OAuthAuthenticationChallengeHandler oauthAuthenticationChallengeHandler;

	void Start()
    {
#if (UNITY_ANDROID || UNITY_IOS || UNITY_WSA) && !UNITY_EDITOR
		oauthAuthenticationChallengeHandler = new MobileOAuthAuthenticationChallengeHandler();
#else
		oauthAuthenticationChallengeHandler = new DesktopOAuthAuthenticationChallengeHandler();
#endif

		Esri.ArcGISMapsSDK.Security.AuthenticationChallengeManager.OAuthChallengeHandler = oauthAuthenticationChallengeHandler;
	}

	void OnDestroy()
	{
		if (oauthAuthenticationChallengeHandler != null)
		{
			oauthAuthenticationChallengeHandler.Dispose();
		}
	}
}
