using Esri.Core.SceneComponentProviders;
using Esri.Core.Utils.Math;
using ArcGISMapsSDK.Core.Utils.Math;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArcGISMapsSDK.Core.SceneComponentProviders
{
	public class SceneComponentProvider : ISceneComponentProvider
	{
		private HashSet<SceneComponent> pool = new HashSet<SceneComponent>();
		private List<SceneComponent> free = new List<SceneComponent>();

		private GameObject unused = null;

		private GameObject parent;

		public SceneComponentProvider(int initSize, GameObject parent)
		{
			this.parent = parent;

			unused = new GameObject();
			unused.name = "UnusedPoolGOs";
			unused.transform.SetParent(parent.transform, false);

			for (int i = 0; i < initSize; i++)
			{
				var sceneComponent = new SceneComponent(CreateGameObject(i));
				sceneComponent.SceneComponentGameObject.transform.SetParent(unused.transform, false);
				pool.Add(sceneComponent);
				free.Add(sceneComponent);
			}
		}

		public override ISceneComponent GetSceneComponent()
		{
			if (free.Count > 0)
			{
				var sceneComponent = free[0];
				sceneComponent.SceneComponentGameObject.transform.SetParent(parent.transform, false);
				sceneComponent.SetTransform(Vector3d.Zero, Quaterniond.Identity, 1.0);
				free.RemoveAt(0);
				sceneComponent.SetVisible(true);
				return sceneComponent;
			}
			else
			{
				var sceneComponent = new SceneComponent(CreateGameObject(pool.Count));
				sceneComponent.SceneComponentGameObject.transform.SetParent(parent.transform, false);
				pool.Add(sceneComponent);
				return sceneComponent;
			}
		}

		public override void ReturnSceneComponent(ISceneComponent sceneComponent)
		{
			var sceneComponentInstance = (SceneComponent)sceneComponent;

			sceneComponentInstance.SceneComponentGameObject.transform.SetParent(unused.transform, false);
			sceneComponent.SetVisible(false);
			free.Add(sceneComponentInstance);
		}

		public override void Update(Quaterniond localRotation, double localScale)
		{
			parent.transform.position = Vector3.zero;
			parent.transform.localScale = Vector3.one * (float)localScale;
			parent.transform.rotation = localRotation.ToQuaternion();
		}

		private static GameObject CreateGameObject(int id)
		{
			var gameObject = new GameObject();
			gameObject.name = "GameObjectTile" + id;
			gameObject.SetActive(true);
			var renderer = gameObject.AddComponent<MeshRenderer>();
			renderer.shadowCastingMode = ShadowCastingMode.TwoSided;
			renderer.enabled = false;
			gameObject.AddComponent<MeshFilter>();

			return gameObject;
		}
	}
}
