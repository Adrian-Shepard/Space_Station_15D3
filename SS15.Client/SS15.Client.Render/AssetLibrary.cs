#nullable disable
using Engine.Rendering;
using Silk.NET.OpenGL;
using SS15.Client.Core;
using SS15.Client.Render.GeneratedSprites;

namespace SS15.Client.Render
{
    public class AssetLibrary
    {
        public Texture2D Floor;
        public Texture2D Space;
        public Texture2D WallSingle;
        public Texture2D WallNS;
        public Texture2D WallWE;
        public Texture2D Wire;
        public Texture2D Generator;
        public Texture2D LampOn;
        public Texture2D LampOff;
        public Texture2D DoorOpen;
        public Texture2D DoorClosed;
        public Texture2D Player;
        public Texture2D ArmsPreview;
        public Texture2D Pocket;
        public Texture2D Belt;
        public Texture2D Back;
        public Texture2D Highlight;
        public Texture2D Hand;
        public Texture2D Item;
        public Texture2D White;
        public Texture2D Slot;
        public Texture2D Grid;

        public static AssetLibrary Load(GL gl)
        {
            var a = new AssetLibrary();

            a.Floor = SpriteGenerator.LoadTexture(gl, "Sprites/Tiles/Floors/floor.png", (g) => SpriteGenerator.CreateFloorTexture(g, 32));
            a.Space = SpriteGenerator.LoadTexture(gl, "Sprites/Tiles/Floors/space.png", (g) => SpriteGenerator.CreateSpaceTexture(g, 32));
            a.WallSingle = SpriteGenerator.LoadTexture(gl, "Sprites/Tiles/Walls/Wall_METAL/Wall_METAL_SINGLE.png", (g) => SpriteGenerator.CreateWallTexture(g, 32));
            a.WallNS = SpriteGenerator.LoadTexture(gl, "Sprites/Tiles/Walls/Wall_METAL/Directions/Wall_METAL_North_South.png", (g) => SpriteGenerator.CreateWallTexture(g, 32));
            a.WallWE = SpriteGenerator.LoadTexture(gl, "Sprites/Tiles/Walls/Wall_METAL/Directions/Wall_METAL_West_East.png", (g) => SpriteGenerator.CreateWallTexture(g, 32));
            a.Player = SpriteGenerator.LoadTexture(gl, "Sprites/player.png", (g) => SpriteGenerator.CreatePlayerTexture(g, 32));

            a.Wire = WireSpriteGenerator.LoadOrCreate(gl);
            a.Generator = GeneratorSpriteGenerator.LoadOrCreate(gl);
            a.LampOff = LampSpriteGenerator.LoadOrCreate(gl);
            a.LampOn = LampSpriteGenerator.LoadOrCreateOn(gl);
            a.DoorOpen = DoorSpriteGenerator.LoadOrCreateOpen(gl);
            a.DoorClosed = DoorSpriteGenerator.LoadOrCreateClosed(gl);

            a.ArmsPreview = SpriteGenerator.LoadTexture(gl, "Sprites/Misc/UI/Arms_Preview.png");
            a.Pocket = SpriteGenerator.LoadTexture(gl, "Sprites/Misc/UI/UI_Pocket.png");
            a.Belt = SpriteGenerator.LoadTexture(gl, "Sprites/Misc/UI/UI_Belt.png");
            a.Back = SpriteGenerator.LoadTexture(gl, "Sprites/Misc/UI/UI_Back.png");

            a.Grid = SpriteGenerator.CreateSolidTexture(gl, new byte[] { 0, 0, 0, 255 });
            a.Item = SpriteGenerator.CreateSolidTexture(gl, new byte[] { 255, 255, 255, 255 });
            a.White = SpriteGenerator.CreateSolidTexture(gl, new byte[] { 255, 255, 255, 255 });
            a.Slot = SpriteGenerator.CreateSolidTexture(gl, new byte[] { 50, 50, 50, 255 });
            a.Highlight = SpriteGenerator.CreateSolidTexture(gl, new byte[] { 255, 210, 0, 130 });
            a.Hand = SpriteGenerator.CreateSolidTexture(gl, new byte[] { 240, 200, 170, 255 });

            Logger.Info("Assets", "All textures loaded");
            return a;
        }
    }
}