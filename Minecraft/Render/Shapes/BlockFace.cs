﻿using OpenTK;

namespace Minecraft
{
    struct BlockFace
    {
        public Vector3[] positions;
        public float[] textureCoords;
        public float[] illumination;

        public BlockFace(Vector3[] positions, float[] textureCoords, float[] illumination)
        {
            this.positions = positions;
            this.textureCoords = textureCoords;
            this.illumination = illumination;
        }
    }

    abstract class BlockModel
    {
        public abstract BlockFace[] GetAlwaysVisibleFaces(BlockState state);
        public abstract BlockFace[] GetPartialVisibleFaces(Direction direction, BlockState state);
    }

    abstract class FullBlockModel : BlockModel
    {
        //Counter clock-wise starting bottom-left if facing the face from the front
        protected Vector3[] backFace = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0) };
        protected Vector3[] rightFace = new Vector3[] { new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1) };
        protected Vector3[] frontFace = new Vector3[] { new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1) };
        protected Vector3[] leftFace = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0) };
        protected Vector3[] topFace = new Vector3[] { new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0), new Vector3(0, 1, 0) };
        protected Vector3[] bottomFace = new Vector3[] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1) };

        protected float[] uvBack, uvRight, uvFront, uvLeft, uvTop, uvBottom;

        public FullBlockModel()
        {
            SetStandardUVs();
        }

        public override BlockFace[] GetAlwaysVisibleFaces(BlockState state)
        {
            return new BlockFace[0];
        }

        public override BlockFace[] GetPartialVisibleFaces(Direction direction, BlockState state)
        {
            float[] illumination = new float[4] { 1, 1, 1, 1 };
            switch (direction)
            {
                case Direction.Back: return new BlockFace[] { new BlockFace(backFace, uvBack, illumination) };
                case Direction.Right: return new BlockFace[] { new BlockFace(rightFace, uvRight, illumination) };
                case Direction.Front: return new BlockFace[] { new BlockFace(frontFace, uvFront, illumination) };
                case Direction.Left: return new BlockFace[] { new BlockFace(leftFace, uvLeft, illumination) };
                case Direction.Top: return new BlockFace[] { new BlockFace(topFace, uvTop, illumination) };
                case Direction.Bottom: return new BlockFace[] { new BlockFace(bottomFace, uvBottom, illumination) };
                default: throw new System.Exception("Uncatched face.");
            }
        }

        protected abstract void SetStandardUVs();
    }

    class BlockModelDirt : FullBlockModel
    {
        protected override void SetStandardUVs()
        {
            uvBack = Game.textureAtlas.GetTextureCoords(new Vector2(2, 0));
            uvRight = Game.textureAtlas.GetTextureCoords(new Vector2(2, 0));
            uvFront = Game.textureAtlas.GetTextureCoords(new Vector2(2, 0));
            uvLeft = Game.textureAtlas.GetTextureCoords(new Vector2(2, 0));
            uvTop = Game.textureAtlas.GetTextureCoords(new Vector2(2, 0));
            uvBottom = Game.textureAtlas.GetTextureCoords(new Vector2(2, 0));
        }
    }

    class BlockModelStone : FullBlockModel
    {
        protected override void SetStandardUVs()
        {
            uvBack = Game.textureAtlas.GetTextureCoords(new Vector2(1, 0));
            uvRight = Game.textureAtlas.GetTextureCoords(new Vector2(1, 0));
            uvFront = Game.textureAtlas.GetTextureCoords(new Vector2(1, 0));
            uvLeft = Game.textureAtlas.GetTextureCoords(new Vector2(1, 0));
            uvTop = Game.textureAtlas.GetTextureCoords(new Vector2(1, 0));
            uvBottom = Game.textureAtlas.GetTextureCoords(new Vector2(1, 0));
        }
    }

    abstract class ScissorModel : BlockModel
    {
        protected Vector3[] bladeOneFace = new Vector3[] { new Vector3(1, 0, 1), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 1) };
        protected Vector3[] bladeTwoFace = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(1, 1, 0) };

        protected float[] uvBladeOne, uvBladeTwo;

        public ScissorModel()
        {
            SetStandardUVs();
        }

        public override BlockFace[] GetAlwaysVisibleFaces(BlockState state)
        {
            float[] illumination = new float[4] { 1, 1, 1, 1 };
            return new BlockFace[] {
                new BlockFace(bladeOneFace, uvBladeOne, illumination),
                new BlockFace(bladeTwoFace, uvBladeTwo, illumination)
            };
        }

        public override BlockFace[] GetPartialVisibleFaces(Direction direction, BlockState state)
        {
            return new BlockFace[0];
        }

        protected abstract void SetStandardUVs();
    }

    class BlockModelFlower : ScissorModel
    {
        protected override void SetStandardUVs()
        {
            uvBladeOne = Game.textureAtlas.GetTextureCoords(new Vector2(12, 0));
            uvBladeTwo = Game.textureAtlas.GetTextureCoords(new Vector2(12, 0));
        }
    }
}
