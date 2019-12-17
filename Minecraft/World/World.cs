﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Minecraft
{
    class World
    {
        public static int SeaLevel = 95;

        private WorldGenerator worldGenerator;
        public Dictionary<Vector2, Chunk> loadedChunks = new Dictionary<Vector2, Chunk>();
        private Game game;

        private float secondsPerTick = 0.02F;
        private float elapsedMillisecondsSinceLastTick;
        private List<BlockState> toRemoveBlocks = new List<BlockState>();

        private delegate void OnBlockPlaced(World world, Chunk chunk, BlockState oldState, BlockState newState);
        private event OnBlockPlaced OnBlockPlacedHandler;

        private delegate void OnChunkLoaded(Chunk chunk);
        private event OnChunkLoaded OnChunkLoadedHandler;

        public World(Game game)
        {
            this.game = game;
            worldGenerator = new WorldGenerator();

            OnBlockPlacedHandler += game.masterRenderer.OnBlockPlaced;
            OnChunkLoadedHandler += game.masterRenderer.OnChunkLoaded;
        }

        public void GenerateTestMap(MasterRenderer renderer)
        {
            var start = DateTime.Now;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                   Chunk chunk = worldGenerator.GenerateBlocksForChunkAt(x, y);
                   loadedChunks.Add(new Vector2(x, y), chunk);
                   OnChunkLoadedHandler?.Invoke(chunk);
                }
            }
            var now2 = DateTime.Now - start;
            Console.WriteLine("Generating init chunks took: " + now2 + " s");
        }

        public void Tick(float deltaTime)
        {
            elapsedMillisecondsSinceLastTick += deltaTime;
            if(elapsedMillisecondsSinceLastTick > secondsPerTick)
            {
                foreach (KeyValuePair<Vector2, Chunk> loadedChunk in loadedChunks)
                {
                    loadedChunk.Value.Tick(this, elapsedMillisecondsSinceLastTick);
                }
                elapsedMillisecondsSinceLastTick = 0;
            }

            foreach(BlockState toRemoveBlock in toRemoveBlocks)
            {
                AddBlockToWorld(toRemoveBlock.position, Blocks.Air.GetNewDefaultState());
            }
            toRemoveBlocks.Clear();
        }

        public Vector2 GetChunkPosition(float worldX, float worldZ)
        {
            return new Vector2((int)worldX >> 4, (int)worldZ >> 4);
        }

        public void DeleteBlockAt(Vector3 intPosition)
        {
            toRemoveBlocks.Add(GetBlockAt(intPosition));
        }

        public bool AddBlockToWorld(Vector3 intPosition, BlockState blockstate)
        {
            return AddBlockToWorld((int)intPosition.X, (int)intPosition.Y, (int)intPosition.Z, blockstate);
        }

        public bool AddBlockToWorld(int worldX, int worldY, int worldZ, BlockState newBlockState)
        {
            newBlockState.position = new Vector3(worldX, worldY, worldZ);

            if (IsOutsideBuildHeight(worldY))
            {
                return false;
            }

            Vector2 chunkPos = GetChunkPosition(worldX, worldZ);
            if(!loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                Console.WriteLine("Tried to add block in junk that doesnt exist");
                return false;
            }

            if(newBlockState.block.GetCollisionBox(newBlockState).Any(aabb => game.player.hitbox.Intersects(aabb)))
            {
                Console.WriteLine("Block tried to placed was in player");
                return false;
            }

            BlockState oldState = GetBlockAt(worldX, worldY, worldZ);
            if (newBlockState.block != Blocks.Air && oldState.block != Blocks.Air)
            {
                return false;
            }

            int localX = worldX & 15;
            int localZ = worldZ & 15;

            chunk.AddBlock(localX, worldY, localZ, newBlockState);
            newBlockState.block.OnAdded(newBlockState, this);
            OnBlockPlacedHandler?.Invoke(this, chunk, oldState, newBlockState);

            //Console.WriteLine("Changed block at " + worldX + "," + worldY + "," + worldZ + " from " + oldState.block.GetType() + " to " + newBlockState.block.GetType());
            return true;
        }

        public bool IsOutsideBuildHeight(int worldY)
        {
            return worldY < 0 || worldY >= Constants.SECTIONS_IN_CHUNKS * Constants.SECTION_HEIGHT;
        }
        
        public BlockState GetBlockAt(Vector3 worldIntPosition)
        {
            return GetBlockAt((int)worldIntPosition.X, (int)worldIntPosition.Y, (int)worldIntPosition.Z);
        }

        public BlockState GetBlockAt(int worldX, int worldY, int worldZ)
        {
            if (IsOutsideBuildHeight(worldY))
            {
                return Blocks.Air.GetNewDefaultState(); 
            }

            Vector2 chunkPos = GetChunkPosition(worldX, worldZ);
            if (!loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                return Blocks.Air.GetNewDefaultState();
            }

            int sectionHeight = worldY / Constants.SECTION_HEIGHT;
            if(chunk.sections[sectionHeight] == null)
            {
                return Blocks.Air.GetNewDefaultState(); 
            }

            int localX = worldX & 15;    
            int localY = worldY & 15;
            int localZ = worldZ & 15;

            BlockState blockType = chunk.sections[sectionHeight].blocks[localX, localY, localZ];
            if (blockType == null)
            {
                return Blocks.Air.GetNewDefaultState(); 
            }
            return blockType;
        }
    }
}
