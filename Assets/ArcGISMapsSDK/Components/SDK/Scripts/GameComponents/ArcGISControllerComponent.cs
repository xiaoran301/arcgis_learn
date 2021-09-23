using System;
using UnityEngine;


namespace ArcGISMapsSDK.Components
{
	/// <summary>Class that will take care about moving the parent object through the arcgiscoordinates system.</summary>
	[Serializable]
	public class ArcGISControllerComponent : MonoBehaviour
	{
		public bool Collisions { get; set; }

		public bool Active { get; set; }

		public void MoveFoward(float distance)
		{
			//MOVE FOWARD THE ELEMENT DISTANCE IN METERS
		}
	}
}
