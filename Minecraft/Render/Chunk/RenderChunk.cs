﻿using OpenTK;

namespace Minecraft
{
    class RenderChunk
    {
        public VAOModel hardBlocksModel;
        public Matrix4 transformationMatrix { get; private set; }
        public Vector2 gridPosition { get; private set; }

        public RenderChunk(int gridPositionX, int gridPositionZ)
        {
            transformationMatrix = Maths.CreateTransformationMatrix(new Vector3(gridPositionX * Constants.CHUNK_SIZE, 0, gridPositionZ * Constants.CHUNK_SIZE));
            gridPosition = new Vector2(gridPositionX, gridPositionZ);
        }

        public void CleanUp()
        {
            if(hardBlocksModel != null)
            {
                hardBlocksModel.CleanUp();
            }
        }
    }
}
