#nullable disable
using Engine.Rendering;
using Silk.NET.Input;

namespace SS15.Client.Systems
{
    public class HotkeySystem
    {
        private readonly GameContext _ctx;
        private bool _debugPrev;
        private bool _errPanelPrev;
        private bool _clearErrPrev;

        public HotkeySystem(GameContext ctx)
        {
            _ctx = ctx;
        }

        public void Update()
        {
            bool debug = _ctx.Keyboard.IsKeyPressed(Key.Tab);
            if (debug && !_debugPrev)
                _ctx.Hud.DebugVisible = !_ctx.Hud.DebugVisible;
            _debugPrev = debug;

            bool errPanel = _ctx.Keyboard.IsKeyPressed(Key.Escape);
            if (errPanel && !_errPanelPrev)
                _ctx.Hud.ErrorPanelVisible = !_ctx.Hud.ErrorPanelVisible;
            _errPanelPrev = errPanel;

            bool clearErr = _ctx.Keyboard.IsKeyPressed(Key.Delete);
            if (clearErr && !_clearErrPrev)
            {
                _ctx.State.Errors.Clear();
                _ctx.Hud.Notify("Список ошибок очищен", new Color(0.7f, 1f, 0.7f, 1f));
            }
            _clearErrPrev = clearErr;
        }
    }
}