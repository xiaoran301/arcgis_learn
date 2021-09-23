// Unity

using System;
using UnityEngine;

namespace ArcGISMapsSDK.UX
{
    public enum ExtentType
    {
        Square = 0,
        Rectangle = 1,
        Circle = 2
    }

    [Serializable]
    public class Extent
    {
        [SerializeField]
        public double Latitude;

        [SerializeField]
        public double Longitude;

        [SerializeField]
        public float Altitude;

        [SerializeField]
        public double Width;

        [SerializeField]
        public double Length;

        [SerializeField]
        public ExtentType ExtentType = ExtentType.Square;
    }
}
