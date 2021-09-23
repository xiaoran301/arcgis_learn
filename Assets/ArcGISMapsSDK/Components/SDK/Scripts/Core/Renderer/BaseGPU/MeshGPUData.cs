using Esri.Core.Renderer;
using Esri.Core.Utils.Math;
using Unity.Collections;
using UnityEngine;

namespace ArcGISMapsSDK.Core.Renderer
{
	public class MeshGPUData : IMeshGPUData
	{
		public Mesh NativeMesh { get; }

		public MeshGPUData(Mesh mesh)
		{
			NativeMesh = mesh;
		}

		public void Destroy()
		{
			Object.Destroy(NativeMesh);
		}

		public int VertexCount
		{
			get
			{
				return NativeMesh.vertexCount;
			}
		}

		public Vector3f[] Vertices
		{
			get
			{
				var vertices = new NativeArray<Vector3>(NativeMesh.vertices, Allocator.Temp).Reinterpret<Vector3f>(12);
				return vertices.ToArray();
			}
			set
			{
				var vertices = new NativeArray<Vector3f>(value, Allocator.Temp).Reinterpret<Vector3>(12);
				NativeMesh.SetVertices(vertices);
			}
		}

		public Vector3f[] Normals
		{
			get
			{
				var normals = new NativeArray<Vector3>(NativeMesh.normals, Allocator.Temp).Reinterpret<Vector3f>(12);
				return normals.ToArray();
			}
			set
			{
				var normals = new NativeArray<Vector3f>(value, Allocator.Temp).Reinterpret<Vector3>(12);
				NativeMesh.SetNormals(normals);
			}
		}

		public Vector2f[] UVs
		{
			get
			{
				var uvs = new NativeArray<Vector2>(NativeMesh.uv, Allocator.Temp).Reinterpret<Vector2f>(8);
				return uvs.ToArray();
			}
			set
			{
				var uvs = new NativeArray<Vector2f>(value, Allocator.Temp).Reinterpret<Vector2>(8);
				NativeMesh.SetUVs(0, uvs);
			}
		}

		public int[] Triangles
		{
			get
			{
				return NativeMesh.triangles;
			}
			set
			{
				NativeMesh.indexFormat = value.Length > 65536 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
				NativeMesh.triangles = value;
			}
		}

		public void Clear()
		{
			NativeMesh.Clear();
		}

		public void RecalculateBounds()
		{
			NativeMesh.RecalculateBounds();
		}

		public void MarkDynamic()
		{
			NativeMesh.MarkDynamic();
		}

		public void SetUVs<T>(int channel, T[] uvs) where T : struct
		{
			var uvsArray = new NativeArray<T>(uvs, Allocator.Temp);
			NativeMesh.SetUVs<T>(channel, uvsArray);
		}
	}
}
