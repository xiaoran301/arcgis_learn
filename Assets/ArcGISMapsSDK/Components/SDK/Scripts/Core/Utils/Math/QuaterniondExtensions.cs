using Esri.Core.Utils.Math;
using UnityEngine;

namespace ArcGISMapsSDK.Core.Utils.Math
{
    public static class QuaterniondExtensions
    {
        public static Quaternion ToQuaternion(this Quaterniond value)
        {
            return new Quaternion((float)value.x, (float)value.y, (float)value.z, (float)value.w);
        }

        public static Quaterniond ToQuaterniond(this Quaternion value)
        {
            return new Quaterniond(value.x, value.y, value.z, value.w);
        }
    }
}
