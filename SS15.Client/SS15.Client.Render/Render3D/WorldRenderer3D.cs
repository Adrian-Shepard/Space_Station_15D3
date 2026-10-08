#nullable disable
using Silk.NET.OpenGL;
using System;
using System.Numerics;
using Engine.Rendering;
using SS15.Client.Core;
using SS15.Client.Systems;

namespace SS15.Client.Render
{
    public class WorldRenderer3D
    {
        private GL _gl;
        private GameMap _map;
        private PowerSystem _powerSystem;
        private AtmosphereSystem _atmosphereSystem;

        private MeshRenderer _meshRenderer;

        // Меши
        private Mesh _cubeMesh;
        private Mesh _floorMesh;
        private Mesh _wireCenter;
        private Mesh _wireArmN, _wireArmS, _wireArmW, _wireArmE;
        private Mesh _wallFaceN, _wallFaceS, _wallFaceW, _wallFaceE;
        private Mesh _capFaceN, _capFaceS, _capFaceW, _capFaceE;
        private Mesh _highlightMesh;

        // Текстуры
        private Texture2D _wallTex;
        private Texture2D _wallCapTex;
        private Texture2D _floorTex, _spaceTex;
        private Texture2D _wireTex;
        private Texture2D _doorOpenTex, _doorClosedTex;
        private Texture2D _generatorTex, _lampOnTex, _lampOffTex;
        private Texture2D _highlightTex;

        private const float WallHeight = 2.4f;
        private const float DoorHeight = 2.0f;
        private const float DoorCapY = DoorHeight;
        private const float MachineHeight = 0.6f;

        // Провод чуть над полом
        private const float WireHeight = 0.02f;

        private const int ViewRadius = 20;

        private bool _loggedFirstDraw = false;
        private int _lastDrawnTiles = 0;

        private static readonly Vector3 FloorScale = Vector3.One;
        private static readonly Vector3 WireScale = Vector3.One;
        private static readonly Vector3 WallFaceScale = new Vector3(1f, WallHeight, 1f);
        private static readonly Vector3 CapFaceScale = Vector3.One;
        private static readonly Vector3 DoorScale = new Vector3(1f, DoorHeight, 1f);
        private static readonly Vector3 GenScale = new Vector3(0.8f, MachineHeight, 0.8f);
        private static readonly Vector3 LampScale = new Vector3(0.5f, MachineHeight, 0.5f);
        private static readonly Vector3 GenOffset = new Vector3(0.1f, 0f, 0.1f);
        private static readonly Vector3 LampOffset = new Vector3(0.25f, 0f, 0.25f);

        private const float HighlightSize = 0.96f;
        private const float HighlightThinY = 0.015f;

        public WorldRenderer3D(GL gl, GameMap map,
            PowerSystem power, AtmosphereSystem atmos)
        {
            _gl = gl;
            _map = map;
            _powerSystem = power;
            _atmosphereSystem = atmos;

            Logger.Info("World3D", "Creating Shader3D...");
            var shader = new Shader3D(gl);
            _meshRenderer = new MeshRenderer(gl, shader);

            Logger.Info("World3D", "Building meshes...");
            _cubeMesh = TileMeshBuilder.BuildUnitCube(gl);
            _floorMesh = TileMeshBuilder.BuildFloorQuad(gl);

            _wireCenter = TileMeshBuilder.BuildWireCenter(gl);
            _wireArmN = TileMeshBuilder.BuildWireArmN(gl);
            _wireArmS = TileMeshBuilder.BuildWireArmS(gl);
            _wireArmW = TileMeshBuilder.BuildWireArmW(gl);
            _wireArmE = TileMeshBuilder.BuildWireArmE(gl);

            _wallFaceN = TileMeshBuilder.BuildWallFaceN(gl);
            _wallFaceS = TileMeshBuilder.BuildWallFaceS(gl);
            _wallFaceW = TileMeshBuilder.BuildWallFaceW(gl);
            _wallFaceE = TileMeshBuilder.BuildWallFaceE(gl);

            _capFaceN = TileMeshBuilder.BuildCapFaceN(gl);
            _capFaceS = TileMeshBuilder.BuildCapFaceS(gl);
            _capFaceW = TileMeshBuilder.BuildCapFaceW(gl);
            _capFaceE = TileMeshBuilder.BuildCapFaceE(gl);

            _highlightMesh = TileMeshBuilder.BuildHighlightBox(gl);

            Logger.Info("World3D", "WorldRenderer3D initialized");
        }

        public void SetTextures(
            Texture2D wallSingle, Texture2D wallNS, Texture2D wallWE,
            Texture2D floor, Texture2D space,
            Texture2D wire,
            Texture2D doorOpen, Texture2D doorClosed,
            Texture2D generator, Texture2D lampOn, Texture2D lampOff,
            Texture2D highlight)
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

            Logger.Info("World3D", $"Textures assigned. Wall={_wallTex?.Handle ?? 0}, " +
                $"Wire={_wireTex?.Handle ?? 0}, Highlight={_highlightTex?.Handle ?? 0}");
        }

        public void Draw(Camera3D camera, RaycastHit hit)
        {
            float camX = camera.Position.X;
            float camZ = camera.Position.Z;

            float fwdX = MathF.Cos(camera.Yaw);
            float fwdZ = MathF.Sin(camera.Yaw);

            if (!_loggedFirstDraw)
            {
                _loggedFirstDraw = true;
                Logger.Info("World3D", $"First Draw. Camera=({camX:F1},{camera.Position.Y:F1},{camZ:F1}) Viewport={camera.ViewportWidth}x{camera.ViewportHeight}");
                Logger.Info("World3D", $"Yaw={camera.Yaw:F2} Pitch={camera.Pitch:F2} ViewRadius={ViewRadius}");
                Logger.Info("World3D", $"Tile at camera: ({(int)camX},{(int)camZ}) = {_map.GetTile((int)camX, (int)camZ)}");
            }

            _meshRenderer.Begin(
                camera.ViewportWidth, camera.ViewportHeight,
                camera.GetViewMatrix(), camera.GetProjectionMatrix(),
                camera.Position);

            int cx = (int)MathF.Floor(camX);
            int cz = (int)MathF.Floor(camZ);

            int x0 = Math.Max(0, cx - ViewRadius);
            int x1 = Math.Min(_map.Width - 1, cx + ViewRadius);
            int z0 = Math.Max(0, cz - ViewRadius);
            int z1 = Math.Min(_map.Height - 1, cz + ViewRadius);

            int drawn = 0;

            for (int z = z0; z <= z1; z++)
            {
                int dzBase = z - cz;
                for (int x = x0; x <= x1; x++)
                {
                    int dxBase = x - cx;

                    float dot = dxBase * fwdX + dzBase * fwdZ;
                    int dist2i = dxBase * dxBase + dzBase * dzBase;
                    if (dot < -1f && dist2i > 4) continue;
                    if (dist2i > 25 && dot < MathF.Sqrt(dist2i) * 0.5f) continue;

                    var tile = _map.GetTile(x, z);

                    // Пол
                    if (tile != TileType.Space)
                    {
                        _meshRenderer.Draw(_floorMesh, _floorTex,
                            new Vector3(x, 0f, z), FloorScale);
                        drawn++;
                    }

                    // Провод — центр + рукава к conductive-соседям
                    if (tile == TileType.Wire)
                    {
                        DrawWireTile(x, z);
                        drawn++;
                        continue;
                    }

                    // Стена
                    if (tile == TileType.Wall || tile == TileType.ReinforcedWall)
                    {
                        DrawWallTile(x, z);
                        drawn++;
                        continue;
                    }

                    // Дверь + косяк
                    if (tile == TileType.Door)
                    {
                        _meshRenderer.Draw(_cubeMesh,
                            _map.IsDoorOpen(x, z) ? _doorOpenTex : _doorClosedTex,
                            new Vector3(x, 0f, z), DoorScale);
                        DrawCapTile(x, z);
                        drawn += 2;
                        continue;
                    }

                    // Генератор
                    if (tile == TileType.Generator)
                    {
                        _meshRenderer.Draw(_cubeMesh, _generatorTex,
                            new Vector3(x, 0f, z) + GenOffset, GenScale);
                        drawn++;
                        continue;
                    }

                    // Лампа
                    if (tile == TileType.Lamp)
                    {
                        _meshRenderer.Draw(_cubeMesh,
                            _powerSystem.IsPowered(x, z) ? _lampOnTex : _lampOffTex,
                            new Vector3(x, 0f, z) + LampOffset, LampScale);
                        drawn++;
                        continue;
                    }
                }
            }

            // Подсветка поверх всего
            if (hit.Hit)
            {
                DrawHighlight(hit);
                drawn++;
            }

            _lastDrawnTiles = drawn;
            _meshRenderer.End();
        }

        private void DrawWallTile(int x, int z)
        {
            float fx = x;
            float fz = z;

            if (!IsWall(x, z - 1))
                _meshRenderer.Draw(_wallFaceN, _wallTex, new Vector3(fx, 0f, fz), WallFaceScale);
            if (!IsWall(x, z + 1))
                _meshRenderer.Draw(_wallFaceS, _wallTex, new Vector3(fx, 0f, fz + 1f), WallFaceScale);
            if (!IsWall(x - 1, z))
                _meshRenderer.Draw(_wallFaceW, _wallTex, new Vector3(fx, 0f, fz), WallFaceScale);
            if (!IsWall(x + 1, z))
                _meshRenderer.Draw(_wallFaceE, _wallTex, new Vector3(fx + 1f, 0f, fz), WallFaceScale);
        }

        private void DrawCapTile(int x, int z)
        {
            float fx = x;
            float fz = z;
            float capY = DoorCapY;

            if (!IsWallOrDoor(x, z - 1))
                _meshRenderer.Draw(_capFaceN, _wallCapTex, new Vector3(fx, capY, fz), CapFaceScale);
            if (!IsWallOrDoor(x, z + 1))
                _meshRenderer.Draw(_capFaceS, _wallCapTex, new Vector3(fx, capY, fz + 1f), CapFaceScale);
            if (!IsWallOrDoor(x - 1, z))
                _meshRenderer.Draw(_capFaceW, _wallCapTex, new Vector3(fx, capY, fz), CapFaceScale);
            if (!IsWallOrDoor(x + 1, z))
                _meshRenderer.Draw(_capFaceE, _wallCapTex, new Vector3(fx + 1f, capY, fz), CapFaceScale);
        }

        /// <summary>
        /// Провод: центр всегда, плюс рукава к тем соседям, которые проводят ток.
        /// Проводящие: Wire, Generator, Lamp. Дверь НЕ проводит.
        /// </summary>
        private void DrawWireTile(int x, int z)
        {
            var basePos = new Vector3(x, WireHeight, z);

            // Центр — всегда
            _meshRenderer.Draw(_wireCenter, _wireTex, basePos, WireScale);

            // Рукава — по conductive-соседям
            if (IsConductive(x, z - 1))
                _meshRenderer.Draw(_wireArmN, _wireTex, basePos, WireScale);
            if (IsConductive(x, z + 1))
                _meshRenderer.Draw(_wireArmS, _wireTex, basePos, WireScale);
            if (IsConductive(x - 1, z))
                _meshRenderer.Draw(_wireArmW, _wireTex, basePos, WireScale);
            if (IsConductive(x + 1, z))
                _meshRenderer.Draw(_wireArmE, _wireTex, basePos, WireScale);
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
                case TileType.ReinforcedWall: h = WallHeight; break;
                case TileType.Door:           h = DoorHeight; break;
                case TileType.Generator:      h = MachineHeight; break;
                case TileType.Lamp:           h = MachineHeight; break;
                default:                      h = 0.5f; break;
            }

            var posCube = new Vector3(hit.X + inset, 0f, hit.Z + inset);
            var scaleCube = new Vector3(oversize, h + 0.02f, oversize);
            _meshRenderer.DrawUnlit(_highlightMesh, _highlightTex, posCube, scaleCube);
        }

        public int LastDrawnTiles => _lastDrawnTiles;

        private bool IsWall(int x, int z)
        {
            if (x < 0 || x >= _map.Width || z < 0 || z >= _map.Height) return false;
            var t = _map.GetTile(x, z);
            return t == TileType.Wall || t == TileType.ReinforcedWall;
        }

        private bool IsWallOrDoor(int x, int z)
        {
            if (x < 0 || x >= _map.Width || z < 0 || z >= _map.Height) return false;
            var t = _map.GetTile(x, z);
            return t == TileType.Wall || t == TileType.ReinforcedWall || t == TileType.Door;
        }

        /// <summary>Проводит ли тайл ток. Провода, генераторы и лампы.</summary>
        private bool IsConductive(int x, int z)
        {
            if (x < 0 || x >= _map.Width || z < 0 || z >= _map.Height) return false;
            var t = _map.GetTile(x, z);
            return t == TileType.Wire || t == TileType.Generator || t == TileType.Lamp;
        }
    }
}