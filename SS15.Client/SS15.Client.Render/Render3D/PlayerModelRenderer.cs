#nullable disable
using Silk.NET.OpenGL;
using System;
using System.Numerics;
using Engine.Rendering;
using SS15.Client.Player;
using SS15.Client.Render.Models;
using SS15.Client.Core;
namespace SS15.Client.Render
{
    public class PlayerModelRenderer
    {
        private MeshRenderer _renderer;
        private Texture2D _whiteTex;
        private Model _model;

        private Mesh _unitCube;

        private static readonly Vector3 BodyScale = new Vector3(0.25f, 0.75f, 0.50f);
        private static readonly Vector3 HeadScale = new Vector3(0.50f, 0.50f, 0.50f);
        private static readonly Vector3 LegScale  = new Vector3(0.25f, 0.75f, 0.25f);
        private static readonly Vector3 ArmScale  = new Vector3(0.20f, 0.75f, 0.20f);

        private const float LegHeight = 0.75f;
        private const float BodyHeight = 0.75f;
        private const float HeadHeight = 0.50f;

        private static readonly Color SkinColor  = new Color(0.93f, 0.78f, 0.63f, 1f);
        private static readonly Color HairColor  = new Color(0.28f, 0.20f, 0.14f, 1f);
        private static readonly Color PantsColor = new Color(0.18f, 0.22f, 0.32f, 1f);
        private static readonly Color BootColor  = new Color(0.10f, 0.10f, 0.12f, 1f);

        private const float TileSizeF = 32f;

        public PlayerModelRenderer(GL gl, MeshRenderer renderer, Texture2D whiteTex, Model model = null)
        {
            _renderer = renderer;
            _whiteTex = whiteTex;
            _model = model;

            if (_model == null)
            {
                _unitCube = TileMeshBuilder.BuildUnitCube(gl);
                Logger.Info("PlayerModel", "Using procedural fallback");
            }
            else
            {
                Logger.Info("PlayerModel", $"Using glTF model: {_model.Parts.Count} parts");
            }
        }

        public void Draw(PlayerEntity entity, Camera3D camera)
        {
            if (entity == null) return;

            float dx = entity.Position.X / TileSizeF - camera.Position.X;
            float dz = entity.Position.Y / TileSizeF - camera.Position.Z;
            float d2 = dx * dx + dz * dz;
            if (d2 > 60f * 60f) return;

            float px = entity.Position.X / TileSizeF;
            float pz = entity.Position.Y / TileSizeF;

            var rotation = Matrix4x4.CreateRotationY(-entity.Direction + MathF.PI / 2f);

            if (_model != null)
            {
                DrawModel(px, pz, rotation);
            }
            else
            {
                DrawProcedural(px, pz, entity);
            }
        }

        private void DrawModel(float px, float pz, Matrix4x4 rotation)
        {
            var rootPos = new Vector3(px, _model.Height * 0.5f, pz);

            foreach (var part in _model.Parts)
            {
                if (part.Mesh == null || part.Texture == null) continue;
                _renderer.DrawRotated(part.Mesh, part.Texture, rootPos, Vector3.One, rotation);
            }
        }

        private void DrawProcedural(float px, float pz, PlayerEntity entity)
        {
            float py = 0f;
            var shirtColor = new Color(entity.Color.R, entity.Color.G, entity.Color.B, 1f);

            float legY = py + LegHeight * 0.5f;
            float legSide = BodyScale.Z * 0.5f - LegScale.Z * 0.5f;

            float bootHeight = 0.15f;
            float bootY = py + bootHeight * 0.5f;
            DrawCubeAt(new Vector3(px, bootY, pz + legSide),
                new Vector3(LegScale.X, bootHeight, LegScale.Z), BootColor);
            DrawCubeAt(new Vector3(px, bootY, pz - legSide),
                new Vector3(LegScale.X, bootHeight, LegScale.Z), BootColor);

            float pantsHeight = LegHeight - bootHeight;
            float pantsY = py + bootHeight + pantsHeight * 0.5f;
            DrawCubeAt(new Vector3(px, pantsY, pz + legSide),
                new Vector3(LegScale.X, pantsHeight, LegScale.Z), PantsColor);
            DrawCubeAt(new Vector3(px, pantsY, pz - legSide),
                new Vector3(LegScale.X, pantsHeight, LegScale.Z), PantsColor);

            float bodyY = py + LegHeight + BodyHeight * 0.5f;
            DrawCubeAt(new Vector3(px, bodyY, pz), BodyScale, shirtColor);

            var headFull = new Vector3(HeadScale.X, HeadScale.Y * 0.85f, HeadScale.Z);
            var headSkinCenter = new Vector3(px, py + LegHeight + BodyHeight + HeadHeight * 0.5f - HeadHeight * 0.075f, pz);
            DrawCubeAt(headSkinCenter, headFull, SkinColor);

            var hairScale = new Vector3(HeadScale.X + 0.02f, HeadHeight * 0.20f, HeadScale.Z + 0.02f);
            var hairCenter = new Vector3(px, py + LegHeight + BodyHeight + HeadHeight - hairScale.Y * 0.5f, pz);
            DrawCubeAt(hairCenter, hairScale, HairColor);

            float armY = py + LegHeight + BodyHeight - ArmScale.Y * 0.5f + 0.05f;
            float armSide = BodyScale.Z * 0.5f + ArmScale.Z * 0.5f;
            DrawCubeAt(new Vector3(px, armY, pz + armSide), ArmScale, SkinColor);
            DrawCubeAt(new Vector3(px, armY, pz - armSide), ArmScale, SkinColor);
        }

        private void DrawCubeAt(Vector3 worldCenter, Vector3 scale, Color color)
        {
            var pos = worldCenter - scale * 0.5f;
            _renderer.DrawTinted(_unitCube, _whiteTex, pos, scale, color);
        }
    }
}