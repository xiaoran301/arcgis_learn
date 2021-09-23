using Esri.Core.Utils.Math;
using UnityEngine;

namespace ArcGISMapsSDK.Core.Utils.Math
{
    public static class Vector4fExtensions
    {
        public static Vector4 ToVector4(this Vector4f value)
        {
            return new Vector4(value.x, value.y, value.z, value.w);
        }
    }
}
