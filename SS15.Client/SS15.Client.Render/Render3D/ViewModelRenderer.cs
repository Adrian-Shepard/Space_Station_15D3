#nullable disable
using Silk.NET.OpenGL;
using System;
using System.Numerics;
using Engine.Rendering;
using SS15.Client.Core;
using SS15.Client.Player;

namespace SS15.Client.Render
{
    public class ViewModelRenderer
    {
        private GL _gl;
        private ShaderViewModel _shader;
        private Mesh _cubeMesh;
        private Texture2D _handTex;
        private Texture2D _itemTex;

        // Позиции в camera-space. X: +вправо, Y: +вверх, Z: -вперёд.
        private static readonly Vector3 RightHandPos = new Vector3( 0.42f, -0.52f, -0.75f);
        private static readonly Vector3 LeftHandPos  = new Vector3(-0.42f, -0.52f, -0.75f);

        // Предплечье: длинное по Z, чуть толще чем в Minecraft-масштабе
        private static readonly Vector3 HandScale = new Vector3(0.16f, 0.16f, 0.55f);

        // Наклон — как в Minecraft: рука смотрит «внутрь и вверх»
        private static readonly Vector3 RightHandRotEuler = new Vector3(0.35f,  0.55f, 0.30f);
        private static readonly Vector3 LeftHandRotEuler  = new Vector3(0.35f, -0.55f, -0.30f);

        private static readonly Vector3 ItemScale  = new Vector3(0.18f, 0.18f, 0.18f);
        private static readonly Vector3 ItemOffset = new Vector3(0f, 0.02f, -0.35f);

        private float _bobPhase;
        private Vector2 _bobOffset;
        private const float BobSpeed = 8.5f;
        private const float BobAmplitudeX = 0.025f;
        private const float BobAmplitudeY = 0.012f;
        private const float BobDamping = 12f;

        public ViewModelRenderer(GL gl, Texture2D handTex, Texture2D itemTex)
        {
            _gl = gl;
            _handTex = handTex;
            _itemTex = itemTex;
            _shader = new ShaderViewModel(gl);
            _cubeMesh = TileMeshBuilder.BuildCenteredCube(gl);
            Logger.Info("ViewModel", "Initialized");
        }

        public void Update(float deltaTime, bool isMoving)
        {
            if (isMoving)
            {
                _bobPhase += deltaTime * BobSpeed;
                _bobOffset.X = MathF.Cos(_bobPhase) * BobAmplitudeX;
                _bobOffset.Y = MathF.Sin(_bobPhase * 2f) * BobAmplitudeY;
            }
            else
            {
                float k = Math.Min(1f, deltaTime * BobDamping);
                _bobOffset *= (1f - k);
            }
        }

        public void Render(Camera3D camera, PlayerController player, int activeHandIndex)
        {
            _gl.Clear(ClearBufferMask.DepthBufferBit);
            _gl.Enable(EnableCap.DepthTest);
            _gl.DepthFunc(DepthFunction.Less);
            _gl.DepthMask(true);
            _gl.Disable(EnableCap.CullFace);

            _shader.Use();
            _shader.SetInt("uTexture", 0);
            _shader.SetMatrix4("uProjection", camera.GetProjectionMatrix());

            var leftItem  = player.Inventory.GetItem(0);
            var rightItem = player.Inventory.GetItem(1);

            DrawHand(LeftHandPos,  LeftHandRotEuler,  leftItem,  activeHandIndex == 0, mirror: true);
            DrawHand(RightHandPos, RightHandRotEuler, rightItem, activeHandIndex == 1, mirror: false);

            _gl.Enable(EnableCap.CullFace);
            _gl.DepthMask(true);
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        private void DrawHand(Vector3 basePos, Vector3 baseRotEuler, Item item, bool active, bool mirror)
        {
            float lift = active ? 0.03f : 0f;

            Vector3 pos = basePos
                        + new Vector3(_bobOffset.X * (mirror ? -1f : 1f), _bobOffset.Y, 0f)
                        + new Vector3(0f, lift, 0f);

            // Дополнительный подъём при активности — рука как будто выдвигается
            var rot = new Quaternion(
                MathF.Sin(baseRotEuler.X / 2f), 0f, 0f,
                MathF.Cos(baseRotEuler.X / 2f))
                * Quaternion.CreateFromAxisAngle(Vector3.UnitY, baseRotEuler.Y)
                * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, baseRotEuler.Z);

            var handModel =
                Matrix4x4.CreateScale(HandScale) *
                Matrix4x4.CreateFromQuaternion(rot) *
                Matrix4x4.CreateTranslation(pos);

            _handTex.Bind(TextureUnit.Texture0);
            _shader.SetMatrix4("uModel", handModel);
            _shader.SetVector4("uTint", new Vector4(1f, 0.85f, 0.72f, 1f));
            _cubeMesh.Draw();

            if (item != null)
            {
                var itemModel =
                    Matrix4x4.CreateScale(ItemScale) *
                    Matrix4x4.CreateFromQuaternion(rot) *
                    Matrix4x4.CreateTranslation(pos + Vector3.Transform(ItemOffset, rot));

                _itemTex.Bind(TextureUnit.Texture0);
                _shader.SetMatrix4("uModel", itemModel);

                var c = item.Color;
                _shader.SetVector4("uTint", new Vector4(
                    c.Length > 0 ? c[0] / 255f : 1f,
                    c.Length > 1 ? c[1] / 255f : 1f,
                    c.Length > 2 ? c[2] / 255f : 1f,
                    1f));

                _cubeMesh.Draw();
            }
        }

        public void Dispose()
        {
            _shader?.Dispose();
            _cubeMesh?.Dispose();
        }
    }
}