using Esri.Core.Utils;
using UnityEngine;

namespace ArcGISMapsSDK.Core.Utils
{
	/// <summary>
	/// Implementation of ILog that writes to the Android log.
	/// </summary>
	internal class AndroidLogWrapper : ILog
	{
		private const string Tag = "ArcGISMapsSDK";

		private readonly AndroidJavaClass log = new AndroidJavaClass("android.util.Log");

		/// <summary>
		/// Write a Debug level message to the log.
		/// </summary>
		public void Debug(string message)
		{
			log.CallStatic<int>("d", Tag, message);
		}

		/// <summary>
		/// Write an Info level message to the log.
		/// </summary>
		public void Info(string message)
		{
			log.CallStatic<int>("i", Tag, message);
		}

		/// <summary>
		/// Write a Warning level message to the log.
		/// </summary>
		public void Warning(string message)
		{
			log.CallStatic<int>("w", Tag, message);
		}

		/// <summary>
		/// Write an Error level message to the log.
		/// </summary>
		public void Error(string message)
		{
			log.CallStatic<int>("e", Tag, message);
		}
	}
}
