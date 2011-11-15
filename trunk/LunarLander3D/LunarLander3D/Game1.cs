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
        Video video, video1;
        Texture2D videoTexture;
        bool played;
        enum Screens { INTRO, MENU, GAME, INSTRUCTION };
        Screens currentScreen = Screens.INTRO;
        KeyboardState previousState;
        SpriteFont arial;
        Texture2D telaMenu;
        Menu menu = new Menu();
        int modelScale = 30;
        List<CModel> models = new List<CModel>();
        Camera camera;
        SkySphere sky;
        int cont = 0;

        MouseState lastMouseState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Window.Title = "Lunar Lander 3D";

            IsMouseVisible = false;

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            player = new VideoPlayer();
            spriteBatch = new SpriteBatch(this.GraphicsDevice);
            menu.Initialize(this.Content);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            video = Content.Load<Video>("Lunar3D_Show");
            video1 = Content.Load<Video>("Lunar_menu");
            player = new VideoPlayer();

            arial = Content.Load<SpriteFont>("arial");

            telaMenu = Content.Load<Texture2D>("Graphics/logo_screen");

            models.Add(new CModel(Content.Load<Model>("ground"),
                Vector3.Zero, Vector3.Zero, Vector3.One, GraphicsDevice));

            models.Add(new CModel(Content.Load<Model>("brick_wall"),
                new Vector3(0, -2000,0), new Vector3(0, 0, 0), Vector3.One, GraphicsDevice));

            // Capsula Lunar 2 posição no Array
            models.Add(new CModel(Content.Load<Model>("capsula2"),
                Vector3.Zero, new Vector3(0, 0, 0), 
                new Vector3(modelScale, modelScale, modelScale), 
                GraphicsDevice));

            Effect lightingEffect = Content.Load<Effect>("LightingEffect");
            LightingMaterial lightingMat = new LightingMaterial();

            Effect normalMapEffect = Content.Load<Effect>("NormalMapEffect");
            NormalMapMaterial normalMat = new NormalMapMaterial(
                Content.Load<Texture2D>("brick_normal_map"));
            
            //lightingMat.LightDirection = new Vector3(.5f, .5f, 1); // posição anterior da luz
            lightingMat.LightDirection = new Vector3(-1.5f, .8f, 1);
            lightingMat.LightColor = Vector3.One;

            normalMat.LightDirection = new Vector3(.5f, .5f, 1);
            normalMat.LightColor = Vector3.One;

            models[0].SetModelEffect(lightingEffect, true);
            models[1].SetModelEffect(normalMapEffect, true);

            models[0].Material = lightingMat;
            models[1].Material = normalMat;

            //Camera antiga
            //camera = new FreeCamera(new Vector3(0, 400, 1400),
            //    MathHelper.ToRadians(0),
            //    MathHelper.ToRadians(0),
            //    GraphicsDevice);

            camera = new ChaseCamera(new Vector3(0, 600, 1500), new Vector3(0, 200, 0),
                new Vector3(0, 0, 0), GraphicsDevice);

            sky = new SkySphere(Content, GraphicsDevice,
                Content.Load<TextureCube>("clouds"));

            //sky = new SkySphere(Content, GraphicsDevice,
            //    Content.Load<TextureCube>("test"));

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
            //((FreeCamera)camera).Rotate(deltaX * .005f, deltaY * .005f);

            Vector3 translation = Vector3.Zero;

            // Determine in which direction to move the camera
            //if (keyState.IsKeyDown(Keys.W)) translation += Vector3.Forward;
            //if (keyState.IsKeyDown(Keys.S)) translation += Vector3.Backward;
            //if (keyState.IsKeyDown(Keys.A)) translation += Vector3.Left;
            //if (keyState.IsKeyDown(Keys.D)) translation += Vector3.Right;
            if (keyState.IsKeyDown(Keys.Escape))
            {
                graphics.IsFullScreen = false;
                this.Exit();
            }

            // Move 3 units per millisecond, independent of frame rate
            translation *= 4 *
                (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Move the camera
            //((FreeCamera)camera).Move(translation);

            // Move the camera to the new model's position and orientation
            ((ChaseCamera)camera).Move(models[2].Position, models[2].Rotation);

            // Update the camera
            camera.Update();

            // Update the mouse state
            lastMouseState = mouseState;
        }

        // Capsula Lunar 2 posição no Array
        void updateModel(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            Vector3 rotChange = new Vector3(0, 0, 0);

            // Determine on which axes the ship should be rotated on, if any
            if (keyState.IsKeyDown(Keys.W))
                rotChange += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.S))
                rotChange += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.A))
                rotChange += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.D))
                rotChange += new Vector3(0, -1, 0);

            // Posiciona a Capsula no centro do cenário posição Zero
            if (keyState.IsKeyDown(Keys.Z))
            {
                rotChange = new Vector3(0, 0, 0);
                models[2].Rotation = rotChange;
                models[2].Position = Vector3.Zero;
            }

            // Move no eixo Y para subir
            if (keyState.IsKeyDown(Keys.X))
            {
                models[2].Position += new Vector3(0, 1, 0) *
                    (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
            }

            // Move no eixo Z para avançar
            if (keyState.IsKeyDown(Keys.Up))
            {
                models[2].Position += new Vector3(0, 0, -1) *
                    (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
            }

            // Move no eixo Z para recuar
            if (keyState.IsKeyDown(Keys.Down))
            {
                models[2].Position += new Vector3(0, 0, 1) *
                    (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
            }

            // Move no eixo X para direita
            if (keyState.IsKeyDown(Keys.Right))
            {
                models[2].Position += new Vector3(1, 0, 0) *
                    (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
            }

            // Move no eixo X para esquerda
            if (keyState.IsKeyDown(Keys.Left))
            {
                models[2].Position += new Vector3(-1, 0, 0) *
                    (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
            }

            models[2].Rotation += rotChange * .025f;

            // If space isn't down, the ship shouldn't move
            if (!keyState.IsKeyDown(Keys.Space))
                return;

            // Determine what direction to move in
            Matrix rotation = Matrix.CreateFromYawPitchRoll(
                models[2].Rotation.Y, models[2].Rotation.X, models[2].Rotation.Z);

            // Move in the direction dictated by our rotation matrix
            models[2].Position += Vector3.Transform(Vector3.Forward, rotation)
                * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
        }

        protected override void Update(GameTime gameTime)
        {
            switch (currentScreen)
            {
                case Screens.INTRO:
                    
                    if (player.State == MediaState.Stopped)
                    {
                        played = true;
                        cont++;
                        player.IsLooped = false;
                        player.Play(video);
                    }

                    if (cont > 1)
                    {
                        player.Stop();
                        currentScreen = Screens.MENU;
                        player.IsLooped = true;
                        player.Play(video1);
                    }
                    
                    if (((played) && (player.State == MediaState.Stopped)) || 
                        (Keyboard.GetState().IsKeyDown(Keys.Escape))) 
                    {
                        player.Stop();
                        currentScreen = Screens.MENU;
                        player.IsLooped = true;
                        player.Play(video1);
                    }
                    videoTexture = player.GetTexture();
                    break;

                case Screens.MENU:

                    menu.Update(Keyboard.GetState(), previousState);

                    if (player.State == MediaState.Stopped)
                    {
                        played = true;
                        player.Play(video1);
                        videoTexture = player.GetTexture(); 
                    }

                    if (((played) && (player.State == MediaState.Stopped)) || 
                        (Keyboard.GetState().IsKeyDown(Keys.Enter) && (previousState.IsKeyUp(Keys.Enter))))
                    {
                        player.Stop();
                        switch (menu.Selected)
                        {
                            case Menu.Selection.START:
                                currentScreen = Screens.GAME;
                                menu.Selected = Menu.Selection.NONE;
                                break;
                            case Menu.Selection.EXIT:
                                this.Exit();
                                break;
                            case Menu.Selection.OPTIONS:
                                currentScreen = Screens.INSTRUCTION;
                                menu.Selected = Menu.Selection.NONE;
                                break;
                        }
                    }
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
                    //((FreeCamera)camera).Rotate(deltaX * .005f, deltaY * .005f); // retirado para usar chasecamera

                    Vector3 translation = Vector3.Zero;

                    // Determine in which direction to move the camera
                    //if (keyState.IsKeyDown(Keys.W)) translation += Vector3.Forward;
                    //if (keyState.IsKeyDown(Keys.S)) translation += Vector3.Backward;
                    //if (keyState.IsKeyDown(Keys.A)) translation += Vector3.Left;
                    //if (keyState.IsKeyDown(Keys.D)) translation += Vector3.Right;
                    if (keyState.IsKeyDown(Keys.Escape)) this.Exit();

                    if (keyState.IsKeyDown(Keys.Right))
                    {
                        //models[1].Position.X += 10;
                    }

                    if (keyState.IsKeyDown(Keys.Left))
                    {

                    }

                    // Move 3 units per millisecond, independent of frame rate
                    translation *= 4 *
                        (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    // Move the camera
                    //((FreeCamera)camera).Move(translation);

                    // Update the camera
                    updateModel(gameTime);
                    updateCamera(gameTime);
                    //camera.Update();

                    // Update the mouse state
                    lastMouseState = mouseState;
                    break;

                case Screens.INSTRUCTION:
                    if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                    {
                        currentScreen = Screens.MENU;
                        menu.Selected = Menu.Selection.NONE;
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
                    spriteBatch.DrawString(arial,
                        "Press Escape to skip video",
                        new Vector2(50, 660),
                        Color.Black);
                    spriteBatch.DrawString(arial,
                        "Press Escape to skip video",
                        new Vector2(51, 661),
                        Color.Yellow);
                    break;

                case Screens.MENU:
                    //spriteBatch.Draw(telaMenu, Vector2.Zero, Color.White);
                    spriteBatch.Draw(videoTexture, Vector2.Zero, Color.White);
                    menu.Draw(spriteBatch, arial);
                    break;

                case Screens.GAME:
                    //sky.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);
                    // adiconar nova camera
                    sky.Draw(camera.View, camera.Projection, ((ChaseCamera)camera).Position);

                    spriteBatch.DrawString(arial,
                            "Model Position " + models[2].Position +
                            "\nModel Rotation: " + models[2].Rotation +
                            "\nEsc = Exit",
                            Vector2.Zero,
                            Color.Yellow);

                    foreach (CModel model in models)
                        if (camera.BoundingVolumeIsInView(model.BoundingSphere))
                            //model.Draw(camera.View, camera.Projection, ((FreeCamera)camera).Position);
                            //nova camera
                            model.Draw(camera.View, camera.Projection, ((ChaseCamera)camera).Position);

                    break;

                case Screens.INSTRUCTION:
                    spriteBatch.Draw(telaMenu, Vector2.Zero, Color.White);
                    spriteBatch.DrawString(arial,
                        "Lunar controls:" + 
                        "\nKeys A & D Rotation Y" +
                        "\nKeys W & S Rotation X" +
                        "\nSpacebar = Trust" +
                        "\nEscape = Return to Menu or" +
                        "\n                   Exit in Game",
                        new Vector2(100, 300),
                        Color.Yellow);
                     
                    break;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
