using ArcGISMapsSDK.Components;
using Esri.Core.Utils.GeoCoord;
using Esri.Core.Utils.Math;
using System;
using UnityEngine;

namespace ArcGISMapsSDK.Components
{
	public class ArcGISCameraControllerComponent : MonoBehaviour
	{
		private float TranslationSpeed = 0.0f;
		private float RotationSpeed = 5.0f;
		private double MouseScrollSpeed = 0.1f;

		private static double MaxCameraHeight = 11000000.0;
		private static double MinCameraHeight = 1.8;
		private static double MaxCameraLatitude = 85.0;

		private Vector3d lastCartesianPoint = Vector3d.Zero;
		private MapPoint lastMapPoint = new MapPoint();
		private double lastDotVC = 0.0f;
		private bool firstDragStep = true;

		private Vector3 lastMouseScreenPosition;
		private bool firstOnFocus = true;

		private Esri.GameEngine.Map.ArcGISMapType mapType = Esri.GameEngine.Map.ArcGISMapType.Global;

		public double MaxSpeed = 2000000.0;
		public double MinSpeed = 1000.0;

		private void Awake()
		{
			lastMouseScreenPosition = Input.mousePosition;
			Application.focusChanged += FocusChanged;
		}

		private void Start()
		{
			var renderer = GameObject.FindObjectOfType<ArcGISRendererComponent>();
			var arcGISCamera = gameObject.GetComponent<ArcGISCameraComponent>();

			if (renderer != null && renderer.RendererScene != null && arcGISCamera != null)
			{
				mapType = renderer.RendererScene.MapType;
			}
			else
			{
				Debug.LogError("ArcGISRendererComponent doesn't exist in the scene or RendererView doesn't set in ArcGISRendererComponent or ArcGISCameraComponent doesn't set in controller");
			}
		}

		void Update()
		{
			var arcGISCamera = gameObject.GetComponent<ArcGISCameraComponent>();
			var renderer = GameObject.FindObjectOfType<ArcGISRendererComponent>();

			if (renderer != null && renderer.RendererScene != null && arcGISCamera != null)
			{
				DragMouseEvent();

				var oldENUReference = arcGISCamera.ENUReference;

				var height = arcGISCamera.Height;
				UpdateSpeed(height);

				var forward = new Vector3d(arcGISCamera.Forward.x, arcGISCamera.Forward.y, arcGISCamera.Forward.z);
				var right = new Vector3d(arcGISCamera.Right.x, arcGISCamera.Right.y, arcGISCamera.Right.z);
				var up = new Vector3d(arcGISCamera.Up.x, arcGISCamera.Up.y, arcGISCamera.Up.z);

				var movDir = Vector3d.zero;

				bool changed = false;

				if (Input.GetAxis("Vertical") != 0)
				{
					movDir += forward * Input.GetAxis("Vertical") * TranslationSpeed * Time.deltaTime;
					changed = true;
				}
				if (Input.GetAxis("Horizontal") != 0)
				{
					movDir += right * Input.GetAxis("Horizontal") * TranslationSpeed * Time.deltaTime;
					changed = true;
				}
				if (Input.GetAxis("Jump") != 0)
				{
					movDir += up * Input.GetAxis("Jump") * TranslationSpeed * Time.deltaTime;
					changed = true;
				}
				else if (Input.GetAxis("Submit") != 0)
				{
					movDir -= up * Input.GetAxis("Submit") * TranslationSpeed * Time.deltaTime;
					changed = true;
				}

				if (Input.mouseScrollDelta.y != 0.0)
				{
					var delta = System.Math.Max(1.0, (height - MinCameraHeight)) * MouseScrollSpeed * Input.mouseScrollDelta.y;
					movDir += forward * delta;
					changed = true;
				}

				if (changed)
				{
					var distance = movDir.Length();
					movDir /= distance;

					if (mapType == Esri.GameEngine.Map.ArcGISMapType.Global)
					{
						var nextHeight = (movDir + arcGISCamera.CartesianPosition).Length() - GeoUtils.EarthRadius;

						if (nextHeight > MaxCameraHeight)
						{
							Geometry.RaySphereIntersection(arcGISCamera.CartesianPosition, -movDir, Vector3d.Zero, GeoUtils.EarthRadius + MaxCameraHeight, out var intersection);
							arcGISCamera.CartesianPosition -= movDir * intersection;
						}
						else if (nextHeight < MinCameraHeight)
						{
							Geometry.RaySphereIntersection(arcGISCamera.CartesianPosition, movDir, Vector3d.Zero, GeoUtils.EarthRadius + MinCameraHeight, out var intersection);
							arcGISCamera.CartesianPosition += movDir * intersection;
						}
						else
						{
							arcGISCamera.CartesianPosition += movDir * distance;
						}

						var newENUReference = arcGISCamera.ENUReference;

						arcGISCamera.CartesianRotation = Quaternion.Inverse(Quaternion.LookRotation(oldENUReference.GetColumn(1), -oldENUReference.GetColumn(2))) * arcGISCamera.CartesianRotation;
						arcGISCamera.CartesianRotation = Quaternion.LookRotation(newENUReference.GetColumn(1), -newENUReference.GetColumn(2)) * arcGISCamera.CartesianRotation;
					}
					else
					{
						arcGISCamera.CartesianPosition += movDir * distance;
					}
				}
			}
		}

		public void SetupMaxMinSpeed(double max, double min)
		{
			MaxSpeed = max;
			MinSpeed = min;
		}

		private void DragMouseEvent()
		{
			var deltaMouse = Input.mousePosition - lastMouseScreenPosition;

			if (!firstOnFocus)
			{
				if (Input.GetMouseButton(0))
				{
					if (deltaMouse != Vector3.zero)
					{
						if (mapType == Esri.GameEngine.Map.ArcGISMapType.Global)
						{
							GlobalDragging();
						}
						else if (mapType == Esri.GameEngine.Map.ArcGISMapType.Local)
						{
							LocalDragging();
						}
					}
				}
				else if (Input.GetMouseButton(1))
				{
					if (!deltaMouse.Equals(Vector3.zero))
					{
						RotateAround(deltaMouse);
					}
				}
				else if (!Input.GetMouseButton(0))
				{
					firstDragStep = true;
				}
			}
			else
			{
				firstOnFocus = false;
			}

			lastMouseScreenPosition = Input.mousePosition;
		}

		void LocalDragging()
		{
			var arcGISCamera = gameObject.GetComponent<ArcGISCameraComponent>();

			var worldRayDir = GetMouseRayCastDirection();
			var isIntersected = Geometry.RayPlaneIntersection(arcGISCamera.CartesianPosition, worldRayDir, Vector3d.Zero, Vector3d.Up, out var intersection);

			if (isIntersected && intersection >= 0)
			{
				Vector3d cartesianCoord = arcGISCamera.CartesianPosition + worldRayDir * intersection;

				var delta = firstDragStep ? Vector3d.Zero : lastCartesianPoint - cartesianCoord;

				lastCartesianPoint = cartesianCoord + delta;
				arcGISCamera.CartesianPosition += delta;

				firstDragStep = false;
			}
		}

		void GlobalDragging()
		{
			var arcGISCamera = gameObject.GetComponent<ArcGISCameraComponent>();

			var worldRayDir = GetMouseRayCastDirection();
			var isIntersected = Geometry.RaySphereIntersection(arcGISCamera.CartesianPosition, worldRayDir, Vector3d.Zero, GeoUtils.EarthRadius, out var intersection);

			if (isIntersected && intersection >= 0)
			{
				Vector3d cartesianCoord = Vector3d.Normalize(arcGISCamera.CartesianPosition + worldRayDir * intersection);
				var currentMapPoint = GeoUtils.UnityToWGS84(cartesianCoord);

				var visibleHemisphereDir = Vector3d.Normalize(GeoUtils.WGS84ToUnity(new MapPoint(arcGISCamera.Longitude, 0)));

				double dotVC = Vector3d.Dot(cartesianCoord, visibleHemisphereDir);
				lastDotVC = firstDragStep ? dotVC : lastDotVC;

				double deltaLongitude = firstDragStep ? 0 : lastMapPoint.Longitude - currentMapPoint.Longitude;
				double deltaLatitude = firstDragStep ? 0 : lastMapPoint.Latitude - currentMapPoint.Latitude;

				deltaLatitude = Math.Sign(dotVC) != Math.Sign(lastDotVC) ? 0 : deltaLatitude;

				lastMapPoint.Longitude = currentMapPoint.Longitude + deltaLongitude;
				lastMapPoint.Latitude = currentMapPoint.Latitude + deltaLatitude;

				arcGISCamera.Longitude += deltaLongitude;
				var nextLatitude = arcGISCamera.Latitude + (dotVC <= 0 ? -deltaLatitude : deltaLatitude);

				var currentLatitudeLimit = MaxCameraLatitude;

				arcGISCamera.Latitude = Math.Abs(nextLatitude) < currentLatitudeLimit ? nextLatitude : nextLatitude > 0 ? currentLatitudeLimit : -currentLatitudeLimit;

				firstDragStep = false;
				lastDotVC = dotVC;
			}
		}

		void RotateAround(Vector3 deltaMouse)
		{
			var arcGISCamera = gameObject.GetComponent<ArcGISCameraComponent>();
			var ENUReference = arcGISCamera.ENUReference;

			Vector2 angles;

			angles.x = -20 * (deltaMouse.x / (float)Screen.width) * RotationSpeed;
			angles.y = Mathf.Min(Mathf.Max(20 * (deltaMouse.y / (float)Screen.height) * -RotationSpeed, -90.0f), 90.0f);

			var right = Matrix4x4.Rotate(arcGISCamera.CartesianRotation).GetColumn(0);

			var rotationY = Quaternion.AngleAxis(angles.x, ENUReference.GetColumn(2));
			var rotationX = Quaternion.AngleAxis(angles.y, right);

			arcGISCamera.CartesianRotation = rotationY * rotationX * arcGISCamera.CartesianRotation;
		}

		Vector3d GetMouseRayCastDirection()
		{
			var arcGISCamera = gameObject.GetComponent<ArcGISCameraComponent>();

			var forward = arcGISCamera.Forward;
			var right = arcGISCamera.Right;
			var up = arcGISCamera.Up;

			var camera = gameObject.GetComponent<Camera>();

			Matrix4x4d view;
			view.m00 = right.x; view.m01 = up.x; view.m02 = forward.x; view.m03 = 0;
			view.m10 = right.y; view.m11 = up.y; view.m12 = forward.y; view.m13 = 0;
			view.m20 = right.z; view.m21 = up.z; view.m22 = forward.z; view.m23 = 0;
			view.m30 = 0; view.m31 = 0; view.m32 = 0; view.m33 = 1;

			Matrix4x4d proj;
			Matrix4x4 inverseProj = camera.projectionMatrix.inverse;
			proj.m00 = inverseProj.m00; proj.m01 = inverseProj.m01; proj.m02 = inverseProj.m02; proj.m03 = inverseProj.m03;
			proj.m10 = inverseProj.m10; proj.m11 = inverseProj.m11; proj.m12 = inverseProj.m12; proj.m13 = inverseProj.m13;
			proj.m20 = inverseProj.m20; proj.m21 = inverseProj.m21; proj.m22 = inverseProj.m22; proj.m23 = -inverseProj.m23;
			proj.m30 = inverseProj.m30; proj.m31 = inverseProj.m31; proj.m32 = -inverseProj.m32; proj.m33 = inverseProj.m33;

			Vector3d ndcCoord = new Vector3d(2.0 * (Input.mousePosition.x / Screen.width) - 1.0, 2.0 * (Input.mousePosition.y / Screen.height) - 1.0, 1);
			Vector3d viewRayDir = Vector3d.Normalize(proj.MultiplyPoint(ndcCoord));
			return view.MultiplyVector(viewRayDir);
		}

		void FocusChanged(bool isFocus)
		{
			firstOnFocus = true;
		}

		void UpdateSpeed(double height)
		{
			var msMaxSpeed = (MaxSpeed * 1000) / 3600;
			var msMinSpeed = (MinSpeed * 1000) / 3600;
			TranslationSpeed = (float)(Math.Pow(Math.Min((height / 100000.0), 1), 2.0) * (msMaxSpeed - msMinSpeed) + msMinSpeed);
		}
	}
}
