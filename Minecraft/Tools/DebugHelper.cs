﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    class DebugHelper
    {
        private WireframeRenderer wireframeRenderer;
        private Game game;

        private bool renderHitboxes;

        public DebugHelper(Game game, WireframeRenderer wireframeRenderer)
        {
            this.wireframeRenderer = wireframeRenderer;
            this.game = game;
        }

        public void UpdateAndRender()
        {
            if (Game.input.OnKeyPress(OpenTK.Input.Key.F1))
            {
                renderHitboxes = !renderHitboxes;
            }

            Render();
        }

        private void Render()
        {
            if (renderHitboxes)
            {
                foreach(Entity entity in game.world.loadedEntities.Values)
                {
                    AABB aabb = entity.hitbox;
                    float width = Math.Abs(aabb.max.X - aabb.min.X);
                    float length = Math.Abs(aabb.max.Z - aabb.min.Z);
                    float height = Math.Abs(aabb.max.Y - aabb.min.Y);

                    float offset = 0.001f;
                    Vector3 scaleVector = new Vector3(width, height, length);
                    Vector3 translation = entity.position;
                    wireframeRenderer.RenderWireframeAt(2, translation, scaleVector, new Vector3(offset, offset, offset));
                }
            }
        }
    }
}