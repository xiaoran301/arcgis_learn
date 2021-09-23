// Unity

using System;
using UnityEngine;

namespace ArcGISMapsSDK.UX
{
    [Serializable]
    public class Layer
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public string Source;

        [SerializeField]
        public float Opacity;

        [SerializeField]
        public bool Visible;

        public Layer(string name, string source, bool visible, float opacity)
        {
            Name = name;
            Source = source;
            Visible = visible;
            Opacity = opacity;
        }
    }
}
