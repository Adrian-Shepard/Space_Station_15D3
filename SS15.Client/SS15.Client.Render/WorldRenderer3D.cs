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
        private Mesh _cubeMesh;
        private Mesh _floorMesh;

        private Texture2D _wallTex, _wallNSTex, _wallWETex;
        private Texture2D _floorTex, _spaceTex;
        private Texture2D _doorOpenTex, _doorClosedTex;
        private Texture2D _generatorTex, _lampOnTex, _lampOffTex;

        private const float WallHeight = 2.4f;
        private const float DoorHeight = 2.0f;
        private const float MachineHeight = 0.6f;

        // Радиус уменьшен с 30 до 20 — этого достаточно для FPS-вида
        private const int ViewRadius = 20;

        private bool _loggedFirstDraw = false;
        private int _lastDrawnTiles = 0;

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

            Logger.Info("World3D", "Building cube mesh...");
            _cubeMesh = TileMeshBuilder.BuildUnitCube(gl);

            Logger.Info("World3D", "Building floor mesh...");
            _floorMesh = TileMeshBuilder.BuildFloorQuad(gl);

            Logger.Info("World3D", "WorldRenderer3D initialized");
        }

        public void SetTextures(
            Texture2D wallSingle, Texture2D wallNS, Texture2D wallWE,
            Texture2D floor, Texture2D space,
            Texture2D doorOpen, Texture2D doorClosed,
            Texture2D generator, Texture2D lampOn, Texture2D lampOff)
        {
            _wallTex = wallSingle;
            _wallNSTex = wallNS;
            _wallWETex = wallWE;
            _floorTex = floor;
            _spaceTex = space;
            _doorOpenTex = doorOpen;
            _doorClosedTex = doorClosed;
            _generatorTex = generator;
            _lampOnTex = lampOn;
            _lampOffTex = lampOff;

            Logger.Info("World3D", $"Textures assigned. Wall={_wallTex?.Handle ?? 0}, Floor={_floorTex?.Handle ?? 0}");
        }

        public void Draw(Camera3D camera)
        {
            float camX = camera.Position.X;
            float camZ = camera.Position.Z;

            // Направление взгляда (по земле)
            float fwdX = MathF.Cos(camera.Yaw);
            float fwdZ = MathF.Sin(camera.Yaw);

            if (!_loggedFirstDraw)
            {
                _loggedFirstDraw = true;
                Logger.Info("World3D", $"First Draw. Camera=({camX:F1},{camera.Position.Y:F1},{camZ:F1}) Viewport={camera.ViewportWidth}x{camera.ViewportHeight}");
                Logger.Info("World3D", $"Yaw={camera.Yaw:F2} Pitch={camera.Pitch:F2} ViewRadius={ViewRadius}");
                Logger.Info("World3D", $"Tile at camera: ({(int)camX},{(int)camZ}) = {_map.GetTile((int)camX, (int)camZ)}");
            }

            _meshRenderer.Begin(camera.ViewportWidth, camera.ViewportHeight);

            var view = camera.GetViewMatrix();
            var proj = camera.GetProjectionMatrix();

            int cx = (int)Math.Floor(camX);
            int cz = (int)Math.Floor(camZ);

            int x0 = Math.Max(0, cx - ViewRadius);
            int x1 = Math.Min(_map.Width - 1, cx + ViewRadius);
            int z0 = Math.Max(0, cz - ViewRadius);
            int z1 = Math.Min(_map.Height - 1, cz + ViewRadius);

            int drawn = 0;

            for (int z = z0; z <= z1; z++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    // Простейший frustum culling: только тайлы в 90° конусе впереди
                    float dx = x + 0.5f - camX;
                    float dz = z + 0.5f - camZ;
                    float dot = dx * fwdX + dz * fwdZ;
                    float dist2 = dx * dx + dz * dz;

                    // Если тайл далеко сзади или на большом расстоянии — пропускаем
                    // dot < -1 — сзади больше чем на 1 юнит
                    if (dot < -1f && dist2 > 4f) continue;
                    // Коническая проверка: dot / dist > cos(60°) = 0.5
                    if (dist2 > 25f && dot < MathF.Sqrt(dist2) * 0.5f) continue;

                    drawn++;

                    var tile = _map.GetTile(x, z);

                    // Пол
                    var floorModel = Matrix4x4.CreateTranslation(x, 0f, z);
                    var floorTexture = (tile == TileType.Space) ? _spaceTex : _floorTex;
                    _meshRenderer.Draw(_floorMesh, floorTexture, floorModel, view, proj, camera.Position);

                    // Объекты
                    switch (tile)
                    {
                        case TileType.Wall:
                        case TileType.ReinforcedWall:
                            {
                                var model = Matrix4x4.CreateScale(1f, WallHeight, 1f)
                                          * Matrix4x4.CreateTranslation(x, 0f, z);
                                var tex = GetWallTexture(x, z);
                                _meshRenderer.Draw(_cubeMesh, tex, model, view, proj, camera.Position);
                                break;
                            }
                        case TileType.Door:
                            {
                                var model = Matrix4x4.CreateScale(1f, DoorHeight, 1f)
                                          * Matrix4x4.CreateTranslation(x, 0f, z);
                                var tex = _map.IsDoorOpen(x, z) ? _doorOpenTex : _doorClosedTex;
                                _meshRenderer.Draw(_cubeMesh, tex, model, view, proj, camera.Position);
                                break;
                            }
                        case TileType.Generator:
                            {
                                var model = Matrix4x4.CreateScale(0.8f, MachineHeight, 0.8f)
                                          * Matrix4x4.CreateTranslation(x + 0.1f, 0f, z + 0.1f);
                                _meshRenderer.Draw(_cubeMesh, _generatorTex, model, view, proj, camera.Position);
                                break;
                            }
                        case TileType.Lamp:
                            {
                                var model = Matrix4x4.CreateScale(0.5f, MachineHeight, 0.5f)
                                          * Matrix4x4.CreateTranslation(x + 0.25f, 0f, z + 0.25f);
                                var tex = _powerSystem.IsPowered(x, z) ? _lampOnTex : _lampOffTex;
                                _meshRenderer.Draw(_cubeMesh, tex, model, view, proj, camera.Position);
                                break;
                            }
                    }
                }
            }

            _lastDrawnTiles = drawn;
            _meshRenderer.End();
        }

        public int LastDrawnTiles => _lastDrawnTiles;

        private Texture2D GetWallTexture(int x, int z)
        {
            bool north = IsWall(x, z - 1);
            bool south = IsWall(x, z + 1);
            bool west = IsWall(x - 1, z);
            bool east = IsWall(x + 1, z);

            if (north && south && !west && !east) return _wallNSTex;
            if (west && east && !north && !south) return _wallWETex;
            return _wallTex;
        }

        private bool IsWall(int x, int z)
        {
            if (x < 0 || x >= _map.Width || z < 0 || z >= _map.Height) return false;
            var t = _map.GetTile(x, z);
            return t == TileType.Wall || t == TileType.ReinforcedWall || t == TileType.Door;
        }
    }
}