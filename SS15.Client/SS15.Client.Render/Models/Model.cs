#nullable disable
using System.Collections.Generic;
using System.Numerics;
using Engine.Rendering;

namespace SS15.Client.Render.Models
{
    public class ModelPart
    {
        public Mesh Mesh;
        public Texture2D Texture;
    }

    public class Model
    {
        public List<ModelPart> Parts = new();
        public float Height = 1f;
        public float Radius = 0.5f;

        public void Dispose()
        {
            foreach (var p in Parts)
            {
                p.Mesh?.Dispose();
                p.Texture?.Dispose();
            }
            Parts.Clear();
        }
    }
}