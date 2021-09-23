using Esri.Core.Utils.Math;
using UnityEngine;

namespace ArcGISMapsSDK.Core.Utils.Math
{
    public static class Vector3dExtensions
    {
        public static Vector3 ToVector3(this Vector3d value)
        {
            return new Vector3((float)value.x, (float)value.y, (float)value.z);
        }
    }
}

