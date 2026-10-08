#nullable disable
using Silk.NET.OpenGL;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Engine.Rendering;
using SS15.Client.Core;
using SharpGLTF.Schema2;

using GltfTexture = SharpGLTF.Schema2.Texture;

namespace SS15.Client.Render.Models
{
    public static class ModelLoader
    {
        public static Model Load(GL gl, string path, float targetHeight = 2.0f)
        {
            if (!File.Exists(path))
            {
                Logger.Warn("ModelLoader", $"File not found: {path}");
                return null;
            }

            try
            {
                var root = ModelRoot.Load(path);
                return Convert(gl, root, targetHeight);
            }
            catch (Exception ex)
            {
                Logger.Error("ModelLoader", $"Failed to load {path}: {ex.Message}");
                Logger.Error("ModelLoader", $"Stack: {ex.StackTrace}");
                return null;
            }
        }

        private static Model Convert(GL gl, ModelRoot root, float targetHeight)
        {
            var model = new Model();
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);

            var tempParts = new List<(List<Vertex3D> verts, List<uint> idx, Material mat, Vector4? avgVertexColor)>();

            int nodeCount = 0;
            int primTotal = 0;
            int primSkipped = 0;

            foreach (var node in root.LogicalNodes)
            {
                if (node.Mesh == null) continue;
                nodeCount++;

                var world = node.WorldMatrix;

                foreach (var prim in node.Mesh.Primitives)
                {
                    primTotal++;
                    try
                    {
                        var verts = new List<Vertex3D>();
                        var indices = new List<uint>();

                        var posAcc = prim.GetVertexAccessor("POSITION");
                        if (posAcc == null) { primSkipped++; continue; }
                        var posArr = posAcc.AsVector3Array();

                        var normAcc = prim.GetVertexAccessor("NORMAL");
                        var normArr = normAcc?.AsVector3Array();

                        var uvAcc = prim.GetVertexAccessor("TEXCOORD_0");
                        var uvArr = uvAcc?.AsVector2Array();

                        Vector4? avgColor = null;
                        try
                        {
                            var colorAcc = prim.GetVertexAccessor("COLOR_0");
                            if (colorAcc != null)
                            {
                                var cArr = colorAcc.AsVector4Array();
                                var sum = Vector4.Zero;
                                for (int i = 0; i < cArr.Count; i++) sum += cArr[i];
                                if (cArr.Count > 0) avgColor = sum / cArr.Count;
                            }
                        }
                        catch { }

                        for (int i = 0; i < posArr.Count; i++)
                        {
                            var localPos = posArr[i];
                            var worldPos = Vector3.Transform(localPos, world);

                            Vector3 n = new Vector3(0, 1, 0);
                            if (normArr != null && i < normArr.Count)
                            {
                                var localNorm = normArr[i];
                                var worldNorm = Vector3.TransformNormal(localNorm, world);
                                if (worldNorm.LengthSquared() > 0.0001f)
                                    n = Vector3.Normalize(worldNorm);
                            }

                            Vector2 uv = Vector2.Zero;
                            if (uvArr != null && i < uvArr.Count)
                                uv = uvArr[i];

                            verts.Add(new Vertex3D
                            {
                                Position = worldPos,
                                Normal = n,
                                TexCoord = uv,
                            });

                            if (worldPos.X < min.X) min.X = worldPos.X;
                            if (worldPos.Y < min.Y) min.Y = worldPos.Y;
                            if (worldPos.Z < min.Z) min.Z = worldPos.Z;
                            if (worldPos.X > max.X) max.X = worldPos.X;
                            if (worldPos.Y > max.Y) max.Y = worldPos.Y;
                            if (worldPos.Z > max.Z) max.Z = worldPos.Z;
                        }

                        var idxAcc = prim.GetIndexAccessor();
                        if (idxAcc != null)
                        {
                            var idxArr = idxAcc.AsIndicesArray();
                            foreach (var idx in idxArr)
                                indices.Add((uint)idx);
                        }
                        else
                        {
                            for (uint i = 0; i < verts.Count; i++)
                                indices.Add(i);
                        }

                        tempParts.Add((verts, indices, prim.Material, avgColor));
                    }
                    catch (Exception ex)
                    {
                        primSkipped++;
                        Logger.Warn("ModelLoader", $"Primitive skipped: {ex.Message}");
                    }
                }
            }

            Logger.Info("ModelLoader", $"Nodes with mesh: {nodeCount}, primitives total: {primTotal}, ok: {tempParts.Count}, skipped: {primSkipped}");

            if (tempParts.Count == 0)
            {
                Logger.Warn("ModelLoader", "No primitives loaded");
                return null;
            }

            var size = max - min;
            Logger.Info("ModelLoader", $"AABB min=({min.X:F2},{min.Y:F2},{min.Z:F2}) max=({max.X:F2},{max.Y:F2},{max.Z:F2}) size=({size.X:F2},{size.Y:F2},{size.Z:F2})");

            float upSize = size.Y;
            if (upSize < 0.0001f) upSize = 1f;

            var center = (min + max) * 0.5f;
            float scale = targetHeight / upSize;

            Logger.Info("ModelLoader", $"Scale factor: {scale:F4} (targetHeight={targetHeight})");

            int partIdx = 0;
            foreach (var (verts, indices, mat, avgColor) in tempParts)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    var v = verts[i];
                    var p = v.Position;
                    p.X -= center.X;
                    p.Z -= center.Z;
                    p.Y -= min.Y;
                    p.Y -= upSize * 0.5f;
                    v.Position = p * scale;
                    verts[i] = v;
                }

                var mesh = new Mesh(gl, verts.ToArray(), indices.ToArray());
                var tex = SafeLoadTexture(gl, mat, avgColor, partIdx);

                model.Parts.Add(new ModelPart
                {
                    Mesh = mesh,
                    Texture = tex,
                });
                partIdx++;
            }

            model.Height = targetHeight;
            model.Radius = MathF.Max(size.X, size.Z) * 0.5f * scale;

            Logger.Info("ModelLoader", $"Loaded model: {model.Parts.Count} parts, height={model.Height:F2}");

            return model;
        }

        private static Texture2D SafeLoadTexture(GL gl, Material mat, Vector4? avgColor, int partIndex)
        {
            if (mat == null)
            {
                if (avgColor.HasValue) return SolidFromFactor(gl, avgColor.Value);
                return SolidBeige(gl);
            }

            string[] channelNames = { "Diffuse", "BaseColor", "Emissive" };
            foreach (var name in channelNames)
            {
                try
                {
                    var ch = mat.FindChannel(name);
                    if (!ch.HasValue) continue;

                    var tex = ch.Value.Texture;
                    if (tex == null) continue;

                    var decoded = TryDecodeAnyChannel(gl, tex, partIndex, mat.Name, name);
                    if (decoded != null) return decoded;
                }
                catch (Exception ex)
                {
                    Logger.Warn("ModelLoader", $"[part {partIndex}] channel '{name}': {ex.Message}");
                }
            }

            try
            {
                var bc = mat.FindChannel("BaseColor");
                if (bc.HasValue)
                {
                    var f = bc.Value.Color;
                    if (f.X > 0.01f || f.Y > 0.01f || f.Z > 0.01f)
                    {
                        Logger.Info("ModelLoader", $"[part {partIndex}] using factor");
                        return SolidFromFactor(gl, f);
                    }
                }
            }
            catch { }

            if (avgColor.HasValue)
                return SolidFromFactor(gl, avgColor.Value);

            Logger.Info("ModelLoader", $"[part {partIndex}] using beige fallback");
            return SolidBeige(gl);
        }

        private static Texture2D TryDecodeAnyChannel(GL gl, GltfTexture tex, int partIndex, string matName, string channelName)
        {
            byte[] bytes = null;
            try
            {
                var img = tex.PrimaryImage;
                if (img == null) return null;

                var content = img.Content;
                if (content.Content.Length == 0) return null;

                bytes = content.Content.ToArray();
            }
            catch (Exception ex)
            {
                Logger.Warn("ModelLoader", $"[part {partIndex}] '{matName}' [{channelName}]: content error: {ex.Message}");
                return null;
            }

            if (bytes == null || bytes.Length < 4)
                return null;

            Logger.Info("ModelLoader", $"[part {partIndex}] '{matName}' [{channelName}]: bytes={bytes.Length} first4={bytes[0]:X2}{bytes[1]:X2}{bytes[2]:X2}{bytes[3]:X2}");

            bool looksLikeImage =
                (bytes[0] == 0x89 && bytes[1] == 0x50) ||
                (bytes[0] == 0xFF && bytes[1] == 0xD8) ||
                (bytes[0] == 0x47 && bytes[1] == 0x49) ||
                (bytes[0] == 0x42 && bytes[1] == 0x4D);

            if (!looksLikeImage)
            {
                Logger.Warn("ModelLoader", $"[part {partIndex}] '{matName}' [{channelName}]: not PNG/JPEG/GIF/BMP, skipping (likely WebP/KTX2)");
                return null;
            }

            try
            {
                using var ms = new MemoryStream(bytes);
                var result = ImageResult.FromStream(ms, ColorComponents.RedGreenBlueAlpha);

                Logger.Info("ModelLoader", $"[part {partIndex}] '{matName}' [{channelName}]: decoded {result.Width}x{result.Height}");

                return ToRgbaTexture(gl, result);
            }
            catch (Exception ex)
            {
                Logger.Warn("ModelLoader", $"[part {partIndex}] '{matName}' [{channelName}]: decode error: {ex.Message}");
                return null;
            }
        }

        private static Texture2D ToRgbaTexture(GL gl, ImageResult result)
        {
            int w = result.Width;
            int h = result.Height;
            int srcChannels = (int)result.Comp;
            byte[] src = result.Data;

            byte[] rgba = new byte[w * h * 4];

            if (srcChannels == 4)
            {
                return new Texture2D(gl, src, w, h, repeat: true);
            }

            if (srcChannels == 3)
            {
                for (int i = 0; i < w * h; i++)
                {
                    rgba[i * 4 + 0] = src[i * 3 + 0];
                    rgba[i * 4 + 1] = src[i * 3 + 1];
                    rgba[i * 4 + 2] = src[i * 3 + 2];
                    rgba[i * 4 + 3] = 255;
                }
                return new Texture2D(gl, rgba, w, h, repeat: true);
            }

            if (srcChannels == 1)
            {
                for (int i = 0; i < w * h; i++)
                {
                    byte g = src[i];
                    rgba[i * 4 + 0] = g;
                    rgba[i * 4 + 1] = g;
                    rgba[i * 4 + 2] = g;
                    rgba[i * 4 + 3] = 255;
                }
                return new Texture2D(gl, rgba, w, h, repeat: true);
            }

            if (srcChannels == 2)
            {
                for (int i = 0; i < w * h; i++)
                {
                    byte g = src[i * 2 + 0];
                    byte a = src[i * 2 + 1];
                    rgba[i * 4 + 0] = g;
                    rgba[i * 4 + 1] = g;
                    rgba[i * 4 + 2] = g;
                    rgba[i * 4 + 3] = a;
                }
                return new Texture2D(gl, rgba, w, h, repeat: true);
            }

            return SolidBeige(gl);
        }

        private static Texture2D SolidFromFactor(GL gl, Vector4 factor)
        {
            byte r = (byte)Math.Clamp(factor.X * 255f, 0, 255);
            byte g = (byte)Math.Clamp(factor.Y * 255f, 0, 255);
            byte b = (byte)Math.Clamp(factor.Z * 255f, 0, 255);
            byte a = (byte)Math.Clamp(factor.W * 255f, 0, 255);
            return SpriteGenerator.CreateSolidTexture(gl, new byte[] { r, g, b, a });
        }

        private static Texture2D SolidBeige(GL gl)
        {
            return SpriteGenerator.CreateSolidTexture(gl, new byte[] { 220, 180, 140, 255 });
        }
    }
}