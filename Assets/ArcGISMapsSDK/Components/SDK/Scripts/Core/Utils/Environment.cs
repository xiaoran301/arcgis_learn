using UnityEngine;

namespace ArcGISMapsSDK.Core.Utils
{
	class Environment
	{
		[RuntimeInitializeOnLoadMethod]
		static void OnRuntimeMethodLoad()
		{
			string productName = Application.productName;
			string productVersion = Application.version;
			string tempDirectory = Application.temporaryCachePath;
			string installDirectory = Application.dataPath;

#if UNITY_ANDROID && !UNITY_EDITOR
			var logger = new AndroidLogWrapper();
#else
			var logger = new UnityLogWrapper();
#endif

			Esri.ArcGISMapsSDKLib.Environment.Initialize(logger, productName, productVersion, tempDirectory, installDirectory);

			Esri.ArcGISMapsSDK.Core.Utils.MainThreadScheduler.Instance();
		}
	}
}
