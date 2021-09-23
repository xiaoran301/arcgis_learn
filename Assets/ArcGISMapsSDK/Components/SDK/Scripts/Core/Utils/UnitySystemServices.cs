using Esri.Core.Utils;
using UnityEngine;

namespace ArcGISMapsSDK.Core.Utils
{
	/// <summary>
	/// Various system services exposed to the ArcGIS plugin protected code.
	/// </summary>
	public class UnitySystemServices : ISystemServices
	{
		/// <summary>
		/// Gets the Unity Debug log.
		/// </summary>
		public ILog Log { get; } = new UnityLogWrapper();

		/// <summary>
		/// MemoryAvailability stats obtained through the Unity API.
		/// </summary>
		public MemoryAvailability GetMemoryAvailability()
		{
			var memoryAvailability = new MemoryAvailability
			{
				TotalSystemMemory = (long)SystemInfo.systemMemorySize * 1024 * 1024,
				TotalVideoMemory = (long)SystemInfo.graphicsMemorySize * 1024 * 1024,
			};
			return memoryAvailability;
		}
	}
}
