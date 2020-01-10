﻿using OpenTK;

namespace Minecraft
{
    class UICanvasIngame : UICanvas
    {
        public UICanvasIngame(Game game) 
            : base(Vector3.Zero, Vector3.Zero, game.window.Width, game.window.Height, RenderSpace.Screen)
        {
            int midX = game.window.Width / 2;
            int midY = game.window.Height / 2;

            Texture cursorTexture = new Texture("../../Resources/cursor.png", 512, 512);
            UIImage cursor = new UIImage(this, new Vector2(midX - 10, midY - 10), new Vector2(20, 20), cursorTexture);
            AddComponentToRender(cursor);
        }

        public override void Update()
        {
            
        }
    }
}
