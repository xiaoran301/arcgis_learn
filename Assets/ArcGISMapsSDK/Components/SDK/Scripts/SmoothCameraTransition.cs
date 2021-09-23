using Esri.Core.Utils.GeoCoord;
using Esri.Core.Utils.Math;
using Esri.GameEngine.Camera;
using Esri.GameEngine.Location;
using UnityEngine;

public class SmoothCameraTransition : MonoBehaviour
{
	public ArcGISCamera Camera;

	private Vector3d dstCartesianCoord;
	private double dstHeight;
	private double highestHeight;
	private double origHeight;
	private Vector3d currentCartesianCoord;
	private Vector3d rotationAxis;
	private double totalDistance;
	private double routeProgress;
	private double speed = (50000.0 / (float)GeoUtils.EarthEquatorLongitude) * 360.0;

	private bool isDirty = false;

	public Vector3 DstCoord
	{
		set
		{
			MapPoint mapPoint;
			var globalPosition = (ArcGISGlobalCoordinatesPosition)Camera.Position;

			dstHeight = value.z;
			origHeight = globalPosition.Altitude;

			mapPoint.Longitude = value.x;
			mapPoint.Latitude = value.y;
			mapPoint.Height = globalPosition.Altitude;
			dstCartesianCoord = Vector3d.Normalize(GeoUtils.WGS84ToUnity(mapPoint));

			mapPoint.Longitude = globalPosition.Latitude;
			mapPoint.Latitude = globalPosition.Longitude;
			currentCartesianCoord = Vector3d.Normalize(GeoUtils.WGS84ToUnity(mapPoint));

			rotationAxis = Vector3d.Normalize(Vector3d.Cross(currentCartesianCoord, dstCartesianCoord));
			routeProgress = totalDistance = System.Math.Acos(Vector3d.Dot(dstCartesianCoord, currentCartesianCoord)) * GeoUtils.Rad2Deg;
			highestHeight = System.Math.Max(dstHeight, origHeight) + (0.5 * (routeProgress / 360.0) * GeoUtils.EarthEquatorLongitude);

			isDirty = true;
		}
	}

	void Update()
	{
		if (Camera != null && isDirty)
		{
			routeProgress -= speed;

			currentCartesianCoord = Matrix4x4d.Rotate(rotationAxis, speed + System.Math.Min(routeProgress, 0)).MultiplyPoint(currentCartesianCoord);

			double progress = 1.0 - routeProgress / totalDistance;

			Vector3d v;
			v.x = currentCartesianCoord.x;
			v.y = currentCartesianCoord.y;
			v.z = currentCartesianCoord.z;

			var coord = GeoUtils.UnityToWGS84(v);

			double alt = ComputeBezier(origHeight, highestHeight, highestHeight, dstHeight, System.Math.Min(progress, 1.0));
			Camera.Position = new ArcGISGlobalCoordinatesPosition(coord.Height, coord.Latitude, coord.Longitude);


			if (routeProgress <= 0)
			{
				isDirty = false;
			}
		}
	}

	static double ComputeBezier(double p0, double p1, double p2, double p3, double t)
	{
		return System.Math.Pow(1.0 - t, 3.0) * p0 + 3.0 * System.Math.Pow(1.0 - t, 2.0) * t * p1 + 3.0 * (1.0 - t) * t * t * p2 + t * t * t * p3;
	}
}
