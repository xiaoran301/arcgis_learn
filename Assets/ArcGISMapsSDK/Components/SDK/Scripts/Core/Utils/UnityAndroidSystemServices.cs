using Esri.Core.Utils;
using System;
using UnityEngine;

namespace ArcGISMapsSDK.Core.Utils
{
	/// <summary>
	/// Various system services exposed to the ArcGIS plugin protected code when running on Android.
	/// </summary>
	internal class UnityAndroidSystemServices : ISystemServices
	{
		/// <summary>
		/// Gets an ILog that writes to the Android log.
		/// </summary>
		public ILog Log { get; } = new AndroidLogWrapper();

		private AndroidJavaObject ActivityManager { get; set; }

		/// <summary>
		/// MemoryAvailability stats obtained through the Android API.
		/// </summary>
		/// <returns>Memory availability figures taken from Android MemoryInfo.</returns>
		public MemoryAvailability GetMemoryAvailability()
		{
			if (TryGetMemoryAvailabilityFromActivityManager(out MemoryAvailability memoryAvailability))
			{
				Log.Debug("Obtained MemoryAvailability from ActivityManager.MemoryInfo");
				return memoryAvailability;
			}
			else
			{
				Log.Debug("Could not get memory availability from ActivityManager.MemoryInfo");
			}

			memoryAvailability.TotalSystemMemory = (long)SystemInfo.systemMemorySize * 1024 * 1024;

			// Unity reports non-zero video memory size for devices with no video memory, so assume a shared memory model on Android by leaving TotalVideoMemory unset.
			// SystemInfo does not provide available/in-use memory stats, but set it to zero to signal that the figures are from SystemInfo
			memoryAvailability.InUseSystemMemory = 0;

			return memoryAvailability;
		}

		private bool TryGetMemoryAvailabilityFromActivityManager(out MemoryAvailability memoryAvailability)
		{
			memoryAvailability = default(MemoryAvailability);

			if (ActivityManager == null)
			{
				AndroidJavaObject currentActivity = null;
				AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				try
				{
					// When running on desktop while set to build Android, this throws
					currentActivity = player.GetStatic<AndroidJavaObject>("currentActivity");
				}
				catch (Exception)
				{
					// Swallow exception
				}

				if (currentActivity != null)
				{
					ActivityManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "activity");
				}
			}

			if (ActivityManager == null)
			{
				return false;
			}

			var memoryInfo = new AndroidJavaObject("android.app.ActivityManager$MemoryInfo");
			ActivityManager.Call("getMemoryInfo", memoryInfo);
			var availableMemory = memoryInfo.Get<long>("availMem");
			var totalMemory = memoryInfo.Get<long>("totalMem");
			var lowMemoryThreshold = memoryInfo.Get<long>("threshold");

			// Treat the difference between available and threshold as "actually available" memory
			var actuallyAvailableMemory = availableMemory - lowMemoryThreshold;
			memoryAvailability.TotalSystemMemory = totalMemory;
			memoryAvailability.InUseSystemMemory = totalMemory - actuallyAvailableMemory;
			return true;
		}
	}
}
