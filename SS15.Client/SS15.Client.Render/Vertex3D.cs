using System.Numerics;
using System.Runtime.InteropServices;

namespace SS15.Client.Render
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex3D
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;
    }
}