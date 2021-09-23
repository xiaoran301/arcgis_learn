using Esri.Core.Utils.Math;
using UnityEngine;

namespace ArcGISMapsSDK.Core.Utils.Math
{
    public static class Matrix4x4dExtensions
    {
        public static Matrix4x4 ToMatrix4x4(this Matrix4x4d value)
        {
            return new Matrix4x4(new Vector4((float)value.m00, (float)value.m10, (float)value.m20, (float)value.m30),
                                new Vector4((float)value.m01, (float)value.m11, (float)value.m21, (float)value.m31),
                                new Vector4((float)value.m02, (float)value.m12, (float)value.m22, (float)value.m32),
                                new Vector4((float)value.m03, (float)value.m13, (float)value.m23, (float)value.m33));
        }
    }
}
