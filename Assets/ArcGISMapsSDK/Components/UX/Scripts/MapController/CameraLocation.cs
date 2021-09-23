// .Net

using System;

// Unity

using UnityEngine;

namespace ArcGISMapsSDK.UX
{
    [Serializable]
    public class CameraLocation
    {
        [SerializeField]
        public double Latitude;

        [SerializeField]
        public double Longitude;

        [SerializeField]
        public float Altitude;

        [SerializeField]
        public float Heading;

        [SerializeField]
        public float Pitch;

        [SerializeField]
        public float Roll;
    }
}
