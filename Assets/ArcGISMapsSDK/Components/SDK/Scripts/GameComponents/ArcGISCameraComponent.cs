using ArcGISMapsSDK.Core.Utils.Math;
using Esri.Core.Utils;
using Esri.Core.Utils.GeoCoord;
using Esri.Core.Utils.Math;
using Esri.GameEngine.Location;
using Esri.GameEngine.View;
using System;
using UnityEngine;

namespace ArcGISMapsSDK.Components
{
    public class ArcGISCameraComponent : MonoBehaviour
	{
		private GameObject Camera = null;

		private bool cartesianDirty = false;
		private bool globalDirty = true;

		private double longitude, latitude, height, heading, pitch, roll;
		private Vector3d cartesianPosition;
		private Quaternion cartesianRotation;

		private Matrix4x4 enuReference = Matrix4x4.identity;

		private Quaternion lastUnityCameraRotation = new Quaternion(0, 1, 0, 0);

		private Scene scene;

		public ArcGISRendererView RendererView;

		public double WorldScale = GeoUtils.EarthRadius;
		public Quaternion WorldRotation = Quaternion.identity;

		public Matrix4x4 ENUReference
		{
			get
			{
				if (cartesianDirty || globalDirty)
				{
					enuReference = scene.GetENUReference(cartesianPosition).ToMatrix4x4();
				}

				return enuReference;
			}

			private set
			{
			}
		}

		public double Near { get; private set; }

		public double Far { get; private set; }

		public double Longitude
		{
			get
			{
				return longitude;
			}
			set
			{
				if (longitude != value)
				{
					longitude = value;
					globalDirty = true;
				}
			}
		}

		public double Latitude
		{
			get
			{
				return latitude;
			}
			set
			{
				if (latitude != value)
				{
					latitude = value;
					globalDirty = true;
				}
			}
		}

		public double Height
		{
			get
			{
				return height;
			}
			set
			{
				if (height != value)
				{
					height = Math.Max(0.01, value);
					globalDirty = true;
				}
			}
		}

		public double Heading
		{
			get
			{
				return heading;
			}
			set
			{
				if (heading != value)
				{
					heading = value;
					globalDirty = true;
				}
			}
		}

		public double Pitch
		{
			get
			{
				return pitch;
			}
			set
			{
				if (pitch != value)
				{
					pitch = value;
					globalDirty = true;
				}
			}
		}

		public double Roll
		{
			get
			{
				return roll;
			}
			set
			{
				if (roll != value)
				{
					roll = value;
					globalDirty = true;
				}
			}
		}

		public Quaternion CartesianRotation
		{
			set
			{
				if (!cartesianRotation.Equals(value))
				{
					cartesianRotation = value;
					cartesianDirty = true;
				}
			}
			get
			{
				return cartesianRotation;
			}
		}

		public Vector3d CartesianPosition
		{
			set
			{
				if (cartesianPosition != value)
				{
					cartesianPosition = value;
					cartesianDirty = true;
				}
			}
			get
			{
				return cartesianPosition;
			}
		}

		public Vector3 Forward
		{
			get
			{
				return CartesianRotation * Vector3.forward;
			}
		}

		public Vector3 Right
		{
			get
			{
				return CartesianRotation * Vector3.right;
			}
		}

		public Vector3 Up
		{
			get
			{
				return CartesianRotation * Vector3.up;
			}
		}

		void Awake()
		{
			Camera = gameObject;

			if (Camera == null || Camera.GetComponent<Camera>() == null)
			{
				Debug.LogError("ArcGISCameraTransform must be assigned to a Camera");
			}
		}

		private void Start()
		{
			var renderer = GameObject.FindObjectOfType<ArcGISRendererComponent>();

			if (renderer != null && RendererView != null)
			{
				scene = renderer.RendererScene;
				WorldScale = scene.MapType == Esri.GameEngine.Map.ArcGISMapType.Global ? GeoUtils.EarthRadius : GeoUtils.EarthEquatorLongitude;

				UpdateCamera();
			}
			else
			{
				Debug.LogError("ArcGISRendererComponent doesn't exist in the scene or RendererView doesn't set in ArcGISRendererComponent");
			}

		}

		private void Update()
		{
			if (RendererView != null)
			{
				UpdateCamera();
			}
		}

		public bool UpdateCamera()
		{
			if (!cartesianDirty && Mathf.Abs(1.0f - Quaternion.Dot(lastUnityCameraRotation, Camera.transform.rotation)) > 1.192093E-07)
			{
				cartesianRotation = Quaternion.Inverse(WorldRotation) * Camera.transform.rotation;
				cartesianDirty = true;
			}

			bool updated = globalDirty || cartesianDirty;

			if (globalDirty || cartesianDirty)
			{
				if (globalDirty)
				{
					RendererView.Camera.Position = new ArcGISGlobalCoordinatesPosition(latitude, longitude, height);
					RendererView.Camera.Orientation = new ArcGISRotation(pitch, roll, heading);

					cartesianPosition = scene.ToCartesianCoord(RendererView.Camera.TransformationMatrix, true);
					cartesianRotation = scene.ToCartesianRotation(RendererView.Camera.TransformationMatrix).ToQuaternion();

					// TODO: This must be remove when Camera API works properly
					if(scene.MapType == Esri.GameEngine.Map.ArcGISMapType.Local)
					{
						RendererView.Camera.TransformationMatrix = scene.ToTransformationMatrix(cartesianPosition, cartesianRotation.ToQuaterniond());
					}
				}
				else if (cartesianDirty)
				{
					RendererView.Camera.TransformationMatrix = scene.ToTransformationMatrix(cartesianPosition, cartesianRotation.ToQuaterniond());

					var coord = scene.ToLatLonAlt(cartesianPosition);
					latitude = coord.Latitude; longitude = coord.Longitude; height = coord.Altitude;
					heading = RendererView.Camera.Orientation.Heading; pitch = RendererView.Camera.Orientation.Pitch; roll = RendererView.Camera.Orientation.Roll;
				}

				enuReference = scene.GetENUReference(cartesianPosition).ToMatrix4x4();
				WorldRotation = scene.GetLocalRotation(cartesianPosition).ToQuaternion();
				Camera.transform.position = Vector3.zero;
				Camera.transform.rotation = WorldRotation * cartesianRotation;

				lastUnityCameraRotation.Set(Camera.transform.rotation.x, Camera.transform.rotation.y, Camera.transform.rotation.z, Camera.transform.rotation.w);

				Near = scene.GetCameraNearPlane(Height, Camera.GetComponent<Camera>().fieldOfView, Camera.GetComponent<Camera>().aspect);
				Far = scene.GetCameraFarPlane(Near, Height);
				Camera.GetComponent<Camera>().farClipPlane = GeoUtils.EarthScaleToUnityScale(Far, scene.MapType == Esri.GameEngine.Map.ArcGISMapType.Local, WorldScale);
				Camera.GetComponent<Camera>().nearClipPlane = GeoUtils.EarthScaleToUnityScale(Math.Min(500000.0, Near), scene.MapType == Esri.GameEngine.Map.ArcGISMapType.Local, WorldScale);

				RendererView.Camera.AspectRatio = Camera.GetComponent<Camera>().aspect;
				RendererView.Camera.FieldOfView = Camera.GetComponent<Camera>().fieldOfView;

				globalDirty = cartesianDirty = false;
			}

			return updated;
		}
	}
}
