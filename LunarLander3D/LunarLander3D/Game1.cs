using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace LunarLander3D
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        VideoPlayer player;
        Video video;
        Texture2D videoTexture;
        bool played;
        enum Screens { INTRO, MENU, GAME };
        Screens currentScreen = Screens.INTRO;
        KeyboardState previousState;
        SpriteFont arial;
        Texture2D telaMenu;

        List<CModel> models = new List<CModel>();
        Camera camera;
        SkySphere sky;

        MouseState lastMouseState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Window.Title = "Lunar Lander 3D";
            IsMouseVisible = false;

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            player = new VideoPlayer();
            spriteBatch = new SpriteBatch(this.GraphicsDevice);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            //video = Content.Load<Video>("video");
            video = Content.Load<Video>("Lunar3D_Show");

            arial = Content.Load<SpriteFont>("arial");

            telaMenu = Content.Load<Texture2D>("telamenu");

            models.Add(new CModel(Content.Load<Model>("ground"),
                Vector3.Zero, Vector3.Zero, Vector3.One, GraphicsDevice));

            models.Add(new CModel(Content.Load<Model>("brick_wall"),
                Vector3.Zero, new Vector3(0, 0, 0), Vector3.One, GraphicsDevice));

            Effect lightingEffect = Content.Load<Effect>("LightingEffect");
            LightingMaterial lightingMat = new LightingMaterial();

            Effect normalMapEffect = Content.Load<Effect>("NormalMapEffect");
            NormalMapMaterial normalMat = new NormalMapMaterial(
                Content.Load<Texture2D>("brick_normal_map"));

            lightingMat.LightDirection = new Vector3(.5f, .5f, 1);
            lightingMat.LightColor = Vector3.One;

            normalMat.LightDirection = new Vector3(.5f, .5f, 1);
            normalMat.LightColor = Vector3.One;

            models[0].SetModelEffect(lightingEffect, true);
            models[1].SetModelEffect(normalMapEffect, true);

            models[0].Material = lightingMat;
            models[1].Material = normalMat;

            camera = new FreeCamera(new Vector3(0, 400, 1400),
                MathHelper.ToRadians(0),
                MathHelper.ToRadians(0),
                GraphicsDevice);

            sky = new SkySphere(Content, GraphicsDevice,
                Content.Load<TextureCube>("clouds"));

            lastMouseState = Mouse.GetState();

        }

        protected override void UnloadContent()
        {
        }

        void updateCamera(GameTime gameTime)
        {
            // Get the new keyboard and mouse state
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

            // Determine how much the camera should turn
            float deltaX = (float)lastMouseState.X - (float)mouseState.X;
            float deltaY = (float)lastMouseState.Y - (float)mouseState.Y;

            // Rotate the camera
            ((FreeCamera)camera).Rotate(deltaX * .005f, deltaY * .005f);

            Vector3 translation = Vector3.Zero;

            // Determine in which direction to move the camera
            if (keyState.IsKeyDown(Keys.W)) translation += Vector3.Forward;
            if (keyState.IsKeyDown(Keys.S)) translation += Vector3.Backward;
            if (keyState.IsKeyDown(Keys.A)) translation += Vector3.Left;
            if (keyState.IsKeyDown(Keys.D)) translation += Vector3.Right;
            if (keyState.IsKeyDown(Keys.Escape)) this.Exit();

            // Move 3 units per millisecond, independent of frame rate
            translation *= 4 *
                (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Move the camera
            ((FreeCamera)camera).Move(translation);

            // Update the camera
            camera.Update();

            // Update the mouse state
            lastMouseState = mouseState;
        }

        protected override void Update(GameTime gameTime)
        {
            switch (currentScreen)
            {
                case Screens.INTRO:
                    
                    if (player.State == MediaState.Stopped)
                    {
                        played = true;
                        player.Play(video);
                    }
                    
                    if (((played) && (player.State == MediaState.Stopped)) || (Keyboard.GetState().IsKeyDown(Keys.Escape))) { player.Stop(); currentScreen = Screens.MENU; }
                    videoTexture = player.GetTexture();
                    break;

                case Screens.GAME:
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape)) this.Exit();
                                MouseState mouseState = Mouse.GetState();

                    KeyboardState keyState = Keyboard.GetState();

                    // Determine how much the camera should turn
                    float deltaX = (float)lastMouseState.X - (float)mouseState.X;
                    float deltaY = (float)lastMouseState.Y - (float)mouseState.Y;

                    // Rotate the camera
                    ((FreeCamera)camera).Rotate(deltaX * .005f, deltaY * .005f);

                    Vector3 translation = Vector3.Zero;

                    // Determine in which direction to move the camera
                    if (keyState.IsKeyDown(Keys.W)) translation += Vector3.Forward;
                    if (keyState.IsKeyDown(Keys.S)) translation += Vector3.Backward;
                    if (keyState.IsKeyDown(Keys.A)) translation += Vector3.Left;
                    if (keyState.IsKeyDown(Keys.D)) translation += Vector3.Right;
                    if (keyState.IsKeyDown(Keys.Escape)) this.Exit();

                    // Move 3 units per millisecond, independent of frame rate
                    translation *= 4 *
                        (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    // Move the camera
                    ((FreeCamera)camera).Move(translation);

                    // Update the camera
                    camera.Update();

                    // Update the mouse state
                    lastMouseState = mouseState;
                    break;

                case Screens.MENU:
                    if (Keyboard.GetState().IsKeyDown(Keys.Enter) && (previousState.IsKeyUp(Keys.Enter))) 
                    {
                        currentScreen = Screens.GAME;
                    }
                    break;

            }


            previousState = Keyboard.GetState();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            switch (currentScreen)
            {
                case Screens.INTRO:
                    spriteBatch.Draw(videoTexture, Vector2.Zero, Color.White);
                    break;

                case Screens.MENU:
                    spriteBatch.Draw(telaMenu, Vector2.Zero, Color.White);
                    break;

                case Screens.GAME:
                    sky.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);

                    foreach (CModel model in models)
                        if (camera.BoundingVolumeIsInView(model.BoundingSphere))
                            model.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);

                    break;

            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
