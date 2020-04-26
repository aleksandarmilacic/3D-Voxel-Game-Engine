﻿namespace Minecraft
{
    class BlockSugarCane : Block
    {
        private readonly float secondsToGrow = 1;
        private readonly int maxLength = 4;

        public BlockSugarCane(ushort id) : base(id)
        {
            isTickable = true;
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateSugarCane();
        }

        public override void OnTick(BlockState blockState, World world, Vector3i blockPos, float deltaTime)
        {
            if(!(world is WorldServer))
            {
                return;
            }

            BlockStateSugarCane caneState = (BlockStateSugarCane)blockState;
            caneState.elapsedTimeSinceLastGrowth += deltaTime;
            if(caneState.elapsedTimeSinceLastGrowth >= secondsToGrow)
            {
                caneState.elapsedTimeSinceLastGrowth = 0;

                BlockState blockAbove = world.GetBlockAt(blockPos.Up());
                if(blockAbove != null && blockAbove.GetBlock() == Blocks.Air)
                {
                    if(GetSugarCaneLength(world, blockPos) < maxLength)
                    {
                       world.QueueToAddBlockAt(blockPos.Up(), GetNewDefaultState());
                    }
                }
            }
        }

        private int GetSugarCaneLength(World world, Vector3i blockPos)
        {
            int length = 1;
            while(world.GetBlockAt(blockPos.Down()).GetBlock() == Blocks.SugarCane)
            {
                length++;
                blockPos = blockPos.Down();
            }
            return length;
        }

        public override void OnDestroy(BlockState blockState, World world, Vector3i blockPos)
        {
            if(!(world is WorldServer))
            {
                return;
            }

            while(world.GetBlockAt(blockPos.Up()).GetBlock() == Blocks.SugarCane)
            {
                world.QueueToRemoveBlockAt(blockPos.Up());
                blockPos = blockPos.Up();
            }
        }

        public override bool CanAddBlockAt(World world, Vector3i blockPos)
        {
            Block blockDown = world.GetBlockAt(blockPos.Down()).GetBlock();
            return blockDown == Blocks.Sand || blockDown == Blocks.Grass || blockDown == Blocks.Dirt;
        }
    }
}
