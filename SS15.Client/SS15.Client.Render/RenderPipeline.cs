#nullable disable
using System;
using Silk.NET.OpenGL;
using SS15.Client.Core;

namespace SS15.Client.Render
{
    public class RenderPipeline
    {
        private readonly GameContext _ctx;

        public RenderPipeline(GameContext ctx)
        {
            _ctx = ctx;
        }

        public void Render()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            _ctx.Framebuffer.Bind();
            _ctx.Gl.Enable(EnableCap.DepthTest);
            _ctx.Gl.DepthFunc(DepthFunction.Less);
            _ctx.Gl.DepthMask(true);
            _ctx.Gl.ClearColor(0.05f, 0.05f, 0.08f, 1f);
            _ctx.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            try { _ctx.World.Draw(_ctx.Camera, _ctx.CurrentHit); }
            catch (Exception ex)
            {
                _ctx.State.Errors.Report("World3D.Draw", ex);
                Logger.Error("World3D.Draw", "Runtime error caught", ex);
            }

            try { _ctx.ViewModel.Render(_ctx.Camera, _ctx.State.Player, _ctx.State.ActiveHandIndex); }
            catch (Exception ex)
            {
                _ctx.State.Errors.Report("ViewModel.Render", ex);
                Logger.Error("ViewModel.Render", "Runtime error caught", ex);
            }

            sw.Stop();
            _ctx.RenderMs = (float)sw.Elapsed.TotalMilliseconds;

            _ctx.Framebuffer.Unbind();
            _ctx.Framebuffer.BlitToScreen((int)_ctx.ScreenWidth, (int)_ctx.ScreenHeight);

            _ctx.Gl.Viewport(0, 0, (uint)_ctx.ScreenWidth, (uint)_ctx.ScreenHeight);
            _ctx.Gl.Disable(EnableCap.DepthTest);
            _ctx.Gl.Disable(EnableCap.CullFace);
            _ctx.Gl.DepthMask(false);
            _ctx.Gl.Enable(EnableCap.Blend);
            _ctx.Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            string hoverName = _ctx.CurrentHit.Hit ? GetTileName(_ctx.CurrentHit.Tile) : "";
            TileType hoverTile = _ctx.CurrentHit.Hit ? _ctx.CurrentHit.Tile : TileType.Floor;

            _ctx.Hud.Draw(
                _ctx.Fps,
                _ctx.PlayerMs, _ctx.AtmosMs, _ctx.PowerMs, _ctx.HealthMs, _ctx.RenderMs,
                _ctx.State.ActiveSlotIndex, _ctx.State.ActiveHandIndex,
                _ctx.ScreenWidth, _ctx.ScreenHeight,
                hoverName,
                _ctx.CurrentHit.Hit, hoverTile,
                _ctx.CurrentHit.X, _ctx.CurrentHit.Z,
                _ctx.State.Runtime,
                _ctx.State.Errors,
                _ctx.Menu);
        }

        private static string GetTileName(TileType tile) => tile switch
        {
            TileType.Floor          => "Floor",
            TileType.Wall           => "Wall",
            TileType.ReinforcedWall => "Reinforced Wall",
            TileType.Wire           => "Wire",
            TileType.Generator      => "Generator",
            TileType.Lamp           => "Lamp",
            TileType.Door           => "Door",
            TileType.Space          => "Space",
            _                       => tile.ToString()
        };
    }
}