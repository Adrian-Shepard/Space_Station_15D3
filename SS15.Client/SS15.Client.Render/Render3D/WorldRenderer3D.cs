#nullable disable
using Silk.NET.OpenGL;
using System;
using System.Numerics;
using Engine.Rendering;
using SS15.Client.Core;
using SS15.Client.Objects.Physics;
using SS15.Client.Player;
using SS15.Client.Render.Models;
using SS15.Client.Systems;

namespace SS15.Client.Render
{
    public class WorldRenderer3D
    {
        private GL _gl;
        private GameMap _map;
        private PowerSystem _powerSystem;
        private AtmosphereSystem _atmosphereSystem;
        private PhysicsWorld _physics;
        private ItemFlightSystem _itemFlight;
        private PlayerEntityList _remotePlayers;
        private PlayerModelRenderer _playerModel;

        private MeshRenderer _meshRenderer;
        private ChunkManager _chunks;

        private Mesh _wireCenter, _wireArmN, _wireArmS, _wireArmW, _wireArmE;
        private Mesh _unitCube;
        private Mesh _highlightMesh;

        private Texture2D _wallTex, _wallCapTex;
        private Texture2D _floorTex, _spaceTex;
        private Texture2D _wireTex;
        private Texture2D _doorOpenTex, _doorClosedTex;
        private Texture2D _generatorTex, _lampOnTex, _lampOffTex;
        private Texture2D _crateTex;
        private Texture2D _itemTex;
        private Texture2D _highlightTex;

        private const float WireHeight = 0.02f;
        private const float MachineHeight = 0.6f;

        private const int ViewRadius = 45;
        private const int CullDist2Min = 16;

        private bool _loggedFirstDraw = false;
        private int _lastDrawnTiles = 0;

        private static readonly Vector3 WireScale = Vector3.One;
        private static readonly Vector3 LampScale = new Vector3(0.5f, MachineHeight, 0.5f);
        private static readonly Vector3 LampOffset = new Vector3(0.25f, 0f, 0.25f);

        private static readonly Vector3 GroundItemScale = new Vector3(0.3f, 0.3f, 0.3f);
        private static readonly Vector3 FlyingItemScale = new Vector3(0.25f, 0.25f, 0.25f);

        private const float HighlightSize = 0.96f;
        private const float HighlightThinY = 0.015f;

        private const float TileSizeF = 32f;

        public WorldRenderer3D(GL gl, GameMap map,
            PowerSystem power, AtmosphereSystem atmos,
            PhysicsWorld physics, ItemFlightSystem itemFlight,
            PlayerEntityList remotePlayers, Texture2D whiteTex,
            Model playerModel)
        {
            _gl = gl;
            _map = map;
            _powerSystem = power;
            _atmosphereSystem = atmos;
            _physics = physics;
            _itemFlight = itemFlight;
            _remotePlayers = remotePlayers;

            var shader = new Shader3D(gl);
            _meshRenderer = new MeshRenderer(gl, shader);

            _wireCenter = TileMeshBuilder.BuildWireCenter(gl);
            _wireArmN = TileMeshBuilder.BuildWireArmN(gl);
            _wireArmS = TileMeshBuilder.BuildWireArmS(gl);
            _wireArmW = TileMeshBuilder.BuildWireArmW(gl);
            _wireArmE = TileMeshBuilder.BuildWireArmE(gl);
            _unitCube = TileMeshBuilder.BuildUnitCube(gl);
            _highlightMesh = TileMeshBuilder.BuildHighlightBox(gl);

            _playerModel = new PlayerModelRenderer(gl, _meshRenderer, whiteTex, playerModel);

            Logger.Info("World3D", $"Initialized (ChunkSize={TileChunk.Size}, ViewRadius={ViewRadius})");
        }

        public void SetTextures(
            Texture2D wallSingle, Texture2D wallNS, Texture2D wallWE,
            Texture2D floor, Texture2D space,
            Texture2D wire,
            Texture2D doorOpen, Texture2D doorClosed,
            Texture2D generator, Texture2D lampOn, Texture2D lampOff,
            Texture2D highlight,
            Texture2D crate,
            Texture2D item)
        {
            _wallTex = wallWE;
            _wallCapTex = wallWE;
            _floorTex = floor;
            _spaceTex = space;
            _wireTex = wire;
            _doorOpenTex = doorOpen;
            _doorClosedTex = doorClosed;
            _generatorTex = generator;
            _lampOnTex = lampOn;
            _lampOffTex = lampOff;
            _highlightTex = highlight;
            _crateTex = crate;
            _itemTex = item;

            var texArray = new Texture2D[]
            {
                _floorTex,
                _wallTex,
                _doorOpenTex,
                _doorClosedTex,
                _generatorTex,
            };
            _chunks = new ChunkManager(_gl, _map, texArray);
        }

        public void Draw(Camera3D camera, RaycastHit hit)
        {
            if (!_loggedFirstDraw)
            {
                _loggedFirstDraw = true;
                Logger.Info("World3D", $"First Draw. ViewRadius={ViewRadius} ChunkSize={TileChunk.Size}");
            }

            _meshRenderer.Begin(
                camera.ViewportWidth, camera.ViewportHeight,
                camera.GetViewMatrix(), camera.GetProjectionMatrix(),
                camera.Position);

            _chunks.DrawVisible(_meshRenderer, camera, ViewRadius);

            DrawDynamic(camera);

            DrawPhysicsObjects();

            DrawGroundItems(camera);

            DrawFlyingItems();

            DrawRemotePlayers(camera);

            if (hit.Hit)
                DrawHighlight(hit);

            _lastDrawnTiles = _chunks.LastDrawnChunks;
            _meshRenderer.End();
        }

        private void DrawDynamic(Camera3D camera)
        {
            float camX = camera.Position.X;
            float camZ = camera.Position.Z;
            float fwdX = MathF.Cos(camera.Yaw);
            float fwdZ = MathF.Sin(camera.Yaw);
            float len = MathF.Sqrt(fwdX * fwdX + fwdZ * fwdZ);
            if (len > 0.001f) { fwdX /= len; fwdZ /= len; }

            int cx = (int)MathF.Floor(camX);
            int cz = (int)MathF.Floor(camZ);

            int x0 = Math.Max(0, cx - ViewRadius);
            int x1 = Math.Min(_map.Width - 1, cx + ViewRadius);
            int z0 = Math.Max(0, cz - ViewRadius);
            int z1 = Math.Min(_map.Height - 1, cz + ViewRadius);
            int R2 = ViewRadius * ViewRadius;

            for (int z = z0; z <= z1; z++)
            {
                int dz = z - cz;
                int dz2 = dz * dz;
                for (int x = x0; x <= x1; x++)
                {
                    int dx = x - cx;
                    int d2 = dx * dx + dz2;
                    if (d2 > R2) continue;

                    if (d2 > CullDist2Min)
                    {
                        float dInv = 1f / MathF.Sqrt(d2);
                        float dotN = (dx * fwdX + dz * fwdZ) * dInv;
                        if (dotN < 0.35f) continue;
                    }

                    var tileType = _map.GetTileType(x, z);

                    if (tileType == TileType.Wire)
                        DrawWireTile(x, z);
                    else if (tileType == TileType.Lamp)
                        DrawLampTile(x, z);
                }
            }
        }

        private void DrawWireTile(int x, int z)
        {
            var basePos = new Vector3(x, WireHeight, z);
            _meshRenderer.Draw(_wireCenter, _wireTex, basePos, WireScale);

            if (IsConductive(x, z - 1))
                _meshRenderer.Draw(_wireArmN, _wireTex, basePos, WireScale);
            if (IsConductive(x, z + 1))
                _meshRenderer.Draw(_wireArmS, _wireTex, basePos, WireScale);
            if (IsConductive(x - 1, z))
                _meshRenderer.Draw(_wireArmW, _wireTex, basePos, WireScale);
            if (IsConductive(x + 1, z))
                _meshRenderer.Draw(_wireArmE, _wireTex, basePos, WireScale);
        }

        private void DrawLampTile(int x, int z)
        {
            var tex = _powerSystem.IsPowered(x, z) ? _lampOnTex : _lampOffTex;
            _meshRenderer.Draw(_unitCube, tex,
                new Vector3(x, 0f, z) + LampOffset, LampScale);
        }

        private void DrawPhysicsObjects()
        {
            if (_physics == null) return;

            var objs = _physics.Objects;
            for (int i = 0; i < objs.Count; i++)
            {
                var o = objs[i];

                float size = o.HalfSize * 2f / TileSizeF;

                var pos = new Vector3(
                    o.Position.X / TileSizeF - size * 0.5f,
                    0f,
                    o.Position.Y / TileSizeF - size * 0.5f);

                var scale = new Vector3(size, o.Height, size);

                Texture2D tex = o.Type == PhysicsObjectType.Crate ? _crateTex : _generatorTex;
                _meshRenderer.Draw(_unitCube, tex, pos, scale);
            }
        }

        private void DrawGroundItems(Camera3D camera)
        {
            if (_itemTex == null) return;

            var items = _map.ItemsOnGround;
            if (items.Count == 0) return;

            float camX = camera.Position.X;
            float camZ = camera.Position.Z;
            float fwdX = MathF.Cos(camera.Yaw);
            float fwdZ = MathF.Sin(camera.Yaw);
            float len = MathF.Sqrt(fwdX * fwdX + fwdZ * fwdZ);
            if (len > 0.001f) { fwdX /= len; fwdZ /= len; }

            int cx = (int)MathF.Floor(camX);
            int cz = (int)MathF.Floor(camZ);
            int R2 = ViewRadius * ViewRadius;

            foreach (var kv in items)
            {
                int tx = kv.Key.x;
                int ty = kv.Key.y;

                int dx = tx - cx;
                int dz = ty - cz;
                int d2 = dx * dx + dz * dz;
                if (d2 > R2) continue;

                if (d2 > CullDist2Min)
                {
                    float dInv = 1f / MathF.Sqrt(d2);
                    float dotN = (dx * fwdX + dz * fwdZ) * dInv;
                    if (dotN < 0.35f) continue;
                }

                var list = kv.Value;
                if (list == null || list.Count == 0) continue;

                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    if (item == null) continue;

                    float ox = 0f, oz = 0f;
                    if (list.Count > 1)
                    {
                        float angle = i * (2f * MathF.PI / list.Count);
                        ox = MathF.Cos(angle) * 0.2f;
                        oz = MathF.Sin(angle) * 0.2f;
                    }

                    var pos = new Vector3(
                        tx + 0.35f + ox,
                        0.15f,
                        ty + 0.35f + oz);

                    var c = item.Color;
                    var col = new Color(
                        c != null && c.Length > 0 ? c[0] / 255f : 1f,
                        c != null && c.Length > 1 ? c[1] / 255f : 1f,
                        c != null && c.Length > 2 ? c[2] / 255f : 1f,
                        1f);

                    _meshRenderer.DrawTinted(_unitCube, _itemTex, pos, GroundItemScale, col);
                }
            }
        }

        private void DrawFlyingItems()
        {
            if (_itemFlight == null || _itemTex == null) return;

            var list = _itemFlight.Items;
            for (int i = 0; i < list.Count; i++)
            {
                var fi = list[i];
                if (fi.Item == null) continue;

                var pos = new Vector3(
                    fi.Position.X / TileSizeF - 0.125f,
                    fi.Height,
                    fi.Position.Y / TileSizeF - 0.125f);

                var c = fi.Item.Color;
                var col = new Color(
                    c != null && c.Length > 0 ? c[0] / 255f : 1f,
                    c != null && c.Length > 1 ? c[1] / 255f : 1f,
                    c != null && c.Length > 2 ? c[2] / 255f : 1f,
                    1f);

                _meshRenderer.DrawTinted(_unitCube, _itemTex, pos, FlyingItemScale, col);
            }
        }

        private void DrawRemotePlayers(Camera3D camera)
        {
            if (_remotePlayers == null || _playerModel == null) return;

            foreach (var kv in _remotePlayers.All)
                _playerModel.Draw(kv.Value, camera);
        }

        private void DrawHighlight(RaycastHit hit)
        {
            bool isFlat = hit.Tile == TileType.Floor
                       || hit.Tile == TileType.Wire
                       || hit.Tile == TileType.Space;

            float oversize = HighlightSize;
            float inset = (1f - oversize) / 2f;

            if (isFlat)
            {
                var pos = new Vector3(hit.X + inset, 0.03f, hit.Z + inset);
                var scale = new Vector3(oversize, HighlightThinY, oversize);
                _meshRenderer.DrawUnlit(_highlightMesh, _highlightTex, pos, scale);
                return;
            }

            float h;
            switch (hit.Tile)
            {
                case TileType.Wall:
                case TileType.ReinforcedWall: h = 2.4f; break;
                case TileType.Door:           h = 2.4f; break;
                case TileType.Generator:      h = MachineHeight; break;
                case TileType.Lamp:           h = MachineHeight; break;
                default:                      h = 0.5f; break;
            }

            var posCube = new Vector3(hit.X + inset, 0f, hit.Z + inset);
            var scaleCube = new Vector3(oversize, h + 0.02f, oversize);
            _meshRenderer.DrawUnlit(_highlightMesh, _highlightTex, posCube, scaleCube);
        }

        public int LastDrawnTiles => _lastDrawnTiles;

        private bool IsConductive(int x, int z)
        {
            if (x < 0 || x >= _map.Width || z < 0 || z >= _map.Height) return false;
            var t = _map.GetTileType(x, z);
            return t == TileType.Wire || t == TileType.Generator || t == TileType.Lamp;
        }
    }
}