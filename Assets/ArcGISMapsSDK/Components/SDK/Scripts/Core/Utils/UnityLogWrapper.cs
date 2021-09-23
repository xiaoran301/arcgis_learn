using Esri.Core.Utils;

namespace ArcGISMapsSDK.Core.Utils
{
	/// <summary>
	/// Implementation of ILog that writes to the Unity DebugLog.
	/// </summary>
	internal class UnityLogWrapper : ILog
	{
		/// <summary>
		/// Write a Debug level message to the log.
		/// </summary>
		public void Debug(string message)
		{
			UnityEngine.Debug.Log(message);
		}

		/// <summary>
		/// Write an Info level message to the log.
		/// </summary>
		public void Info(string message)
		{
			UnityEngine.Debug.Log(message);
		}

		/// <summary>
		/// Write a Warning level message to the log.
		/// </summary>
		public void Warning(string message)
		{
			UnityEngine.Debug.LogWarning(message);
		}

		/// <summary>
		/// Write an Error level message to the log.
		/// </summary>
		public void Error(string message)
		{
			UnityEngine.Debug.LogError(message);
		}
	}
}
