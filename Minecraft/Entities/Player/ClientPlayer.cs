﻿using System;
using OpenTK;
using OpenTK.Input;

namespace Minecraft
{
    class ClientPlayer : Player
    {
        private Game game;

        public Camera camera;
        public RayTraceResult mouseOverObject { get; private set; }

        private BlockState selectedBlock = Blocks.Tnt.GetNewDefaultState();

        private float secondsPerPosUpdate = 0.1F;
        private float elapsedMsSinceLastPosUpdate;

        public ClientPlayer(Game game) : base(-1, "", new Vector3(0, 200, 0))
        {
            this.game = game;
            camera = new Camera(new ProjectionMatrixInfo(0.1F, 1000F, 1.5F, game.window.Width, game.window.Height));

            OnToggleRunningHandler += OnRunningToggle;
            OnToggleCrouchingHandler += OnCrouchingToggle;
        }

        private void OnRunningToggle(bool isRunning)
        {
            if (isRunning)
            {
                camera.SetFieldOfView(1.65F);
            } else
            {
                camera.SetFieldOfViewToDefault();
            }
        }

        private void OnCrouchingToggle(bool isCrouching)
        {
            if (isCrouching)
            {
                camera.SetFieldOfView(1.45F);
            } else
            {
                camera.SetFieldOfViewToDefault();
            }
        }

        public override void Update(float deltaTime, World world)
        {
            UpdateKeyboardInput();
            ApplyVelocityAndCheckCollision(deltaTime, game.world);
            mouseOverObject = new Ray(camera.position, camera.forward).TraceWorld(game.world);

            Vector3 cameraPosition = position;
            cameraPosition.X += Constants.PLAYER_WIDTH / 2.0F;
            cameraPosition.Y += Constants.PLAYER_CAMERA_HEIGHT;
            cameraPosition.Z += Constants.PLAYER_LENGTH / 2.0F;
            camera.SetPosition(cameraPosition);

            if (Game.input.OnMousePress(MouseButton.Right) && game.window.Focused && mouseOverObject != null)
            {
                if (isCrouching)
                {
                    if (selectedBlock.GetBlock().CanAddBlockAt(game.world, mouseOverObject.blockPlacePosition))
                    {
                        BlockState newBlock = selectedBlock.GetBlock().GetNewDefaultState();
                        game.client.WritePacket(new PlaceBlockPacket(newBlock, mouseOverObject.blockPlacePosition));
                    }
                } else
                {
                    BlockState state = game.world.GetBlockAt(mouseOverObject.intersectedBlockPos);
                    if (state.GetBlock().isInteractable)
                    {
                        game.client.WritePacket(new PlayerBlockInteractionPacket(mouseOverObject.intersectedBlockPos));
                    }else if (selectedBlock.GetBlock().CanAddBlockAt(game.world, mouseOverObject.blockPlacePosition))
                    {
                        BlockState newBlock = selectedBlock.GetBlock().GetNewDefaultState();
                        game.client.WritePacket(new PlaceBlockPacket(newBlock, mouseOverObject.blockPlacePosition));
                    }
                }
            }
            if (Game.input.OnMousePress(MouseButton.Middle) && mouseOverObject != null)
            {
                selectedBlock = game.world.GetBlockAt(mouseOverObject.intersectedBlockPos);
            }
            if (Game.input.OnMousePress(MouseButton.Left) && mouseOverObject != null)
            {
                game.client.WritePacket(new RemoveBlockPacket(mouseOverObject.intersectedBlockPos));
            }

            realForward = camera.forward;
            moveForward = new Vector3((float)Math.Sin(camera.pitch), 0, (float)Math.Cos(camera.pitch));
            right = camera.right;

            elapsedMsSinceLastPosUpdate += deltaTime;
            if(elapsedMsSinceLastPosUpdate > secondsPerPosUpdate)
            {
                elapsedMsSinceLastPosUpdate = 0;
                game.client.WritePacket(new PlayerDataPacket(position, id));
            }
        }

        private void UpdateKeyboardInput()
        {
            float speedMultiplier = Constants.PLAYER_BASE_MOVE_SPEED;

            bool wFocused = game.window.Focused;
            bool inputToRun = wFocused && (Game.input.OnKeyDown(Key.ControlLeft) || Game.input.OnKeyDown(Key.ControlRight));
            bool inputToCrouch = wFocused && (Game.input.OnKeyDown(Key.ShiftLeft) || Game.input.OnKeyDown(Key.ShiftRight));
            bool inputToMoveLeft = wFocused && Game.input.OnKeyDown(Key.A);
            bool inputToMoveBack = wFocused && Game.input.OnKeyDown(Key.S);
            bool inputToMoveRight = wFocused && Game.input.OnKeyDown(Key.D);
            bool inputToMoveForward = wFocused && Game.input.OnKeyDown(Key.W);
            bool inputToJump = wFocused && Game.input.OnKeyDown(Key.Space);
            bool inputToFly = wFocused && Game.input.OnKeyPress(Key.Space);

            //Prioritize crouching over running
            if (inputToRun && !inputToCrouch)
            {
                TryStartRunning();
            } else if (inputToCrouch)
            {
                TryStartCrouching();
            } else if (!inputToCrouch)
            {
                TryStopCrouching();
            }

            if (!inputToMoveForward || inputToMoveBack)
            {
                TryStopRunning();
            }

            if (isInAir && !isFlying)
            {
                speedMultiplier *= Constants.PLAYER_IN_AIR_SLOWDOWN;
            }
            if (isFlying)
            {
                speedMultiplier *= Constants.PLAYER_FLYING_MULTIPLIER;
            }

            if (isRunning)
            {
                speedMultiplier *= Constants.PLAYER_SPRINT_MULTIPLIER;
            } else if (isCrouching)
            {
                if (isFlying)
                {
                    MovePlayerVertically(-speedMultiplier);
                } else
                {
                    speedMultiplier *= Constants.PLAYER_CROUCH_MULTIPLIER;
                }
            }

            if (inputToJump)
            {
                if (isFlying)
                {
                    MovePlayerVertically(speedMultiplier);
                } else
                {
                    AttemptToJump();
                }
            }

            if (inputToFly)
            {
                TryToggleFlying();
            }

            if (inputToMoveForward)
            {
                MovePlayerHorizontally(0, speedMultiplier);
            }
            if (inputToMoveBack)
            {
                MovePlayerHorizontally(0, -speedMultiplier);
            }
            if (inputToMoveRight)
            {
                MovePlayerHorizontally(-speedMultiplier, 0);
            }
            if (inputToMoveLeft)
            {
                MovePlayerHorizontally(speedMultiplier, 0);
            }
        }
    }
}
