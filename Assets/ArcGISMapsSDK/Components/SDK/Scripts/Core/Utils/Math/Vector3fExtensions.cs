using Esri.Core.Utils.Math;
using UnityEngine;

namespace ArcGISMapsSDK.Core.Utils.Math
{
    public static class Vector3fExtensions
    {
        public static Vector3 ToVector3(this Vector3f value)
        {
            return new Vector3(value.x, value.y, value.z);
        }
    }
}
