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
        float combustivel = 4000;

        VideoPlayer player;
        Video video, video1;
        Texture2D videoTexture;
        Texture2D mapBorder;
        bool played;

        enum Screens { INTRO, MENU, GAME, INSTRUCTION };
        Screens currentScreen = Screens.INTRO;
        KeyboardState previousState;

        SpriteFont arial;
        Texture2D telaMenu;
        Menu menu = new Menu();

        // Lunar Pod
        int modelScale = 50;
        int index, cont = 0;
        Vector3 LanderDown = new Vector3(500, 4550, -1000); // 500, 2350, -1000
        float gravity = -0.00003f;
        Vector3 shuttleSpeed = Vector3.Zero;
        Vector3 friction = Vector3.Zero; 
        List<CModel> models = new List<CModel>();

        Terrain terrain;
        Camera camera, cameraTop;
        SkySphere sky;

        // Posição inicial da camera  - 0, 600, 1500  // 8000, 6000, 8000 // 0, 400, 1200
        Vector3 cameraPos = new Vector3(100, 450, 1200);
        Vector3 cameraPosTop = new Vector3(0, 1450, 0);

        PrelightingRenderer renderer;

        MouseState lastMouseState;

        /// <summary>
        /// viewport padrao, deve mostrar a cena toda por trás do módulo lunar...
        /// </summary>
        Viewport defaultViewport;

        /// <summary>
        /// viewport do mapa, deve mostrar a cena toda por cima do módulo lunar...
        /// </summary>
        Viewport mapViewport;
        
        /// <summary>
        /// projeção da viewport padrao
        /// </summary>
        Matrix projectionMatrix;

        /// <summary>
        /// projeção da viewport do mapa
        /// </summary>
        Matrix mapProjectionMatrix;

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

            mapBorder = Content.Load<Texture2D>("Graphics/border");

            models.Add(new CModel(Content.Load<Model>("ground"),
                new Vector3(0, -2000, 0), Vector3.Zero, new Vector3(1, 1, 1), GraphicsDevice));

            // 30, 4800 - 30, 9600 - 30, 3200 - 30, 256 (terrains 4 e 5)
            terrain = new Terrain(Content.Load<Texture2D>("Graphics/terrain3"), 30, 3200,
                Content.Load<Texture2D>("Graphics/terrain3"), 6, new Vector3(1, -1, 0), // 6
                GraphicsDevice, Content);

            models.Add(new CModel(Content.Load<Model>("brick_wall"),
                new Vector3(0, -2000,0), new Vector3(0, 0, 0), Vector3.One, GraphicsDevice));

            // Capsula Lunar 2 posição no Array - colocar o index correto - index = 2
            index = 2;
            models.Add(new CModel(Content.Load<Model>("modulo1"),
                LanderDown, Vector3.Zero, 
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

            Effect effect = Content.Load<Effect>("VSM");

            normalMat.LightDirection = new Vector3(.5f, .5f, 1);
            normalMat.LightColor = Vector3.One;

            models[0].SetModelEffect(lightingEffect, true);
            models[1].SetModelEffect(normalMapEffect, true);

            models[0].Material = lightingMat;
            models[1].Material = normalMat;

            // Antes do Shadow
            camera = new ChaseCamera(cameraPos, new Vector3(0, 200, 0), // 0, 200, 0
                new Vector3(0, 0, 0), GraphicsDevice);

            cameraTop = new ChaseCameraRadar(cameraPosTop, new Vector3(0, 0, 0), 
                new Vector3(0, 0, 0), GraphicsDevice);

            sky = new SkySphere(Content, GraphicsDevice,
                Content.Load<TextureCube>("clouds"));

            renderer = new PrelightingRenderer(GraphicsDevice, Content);
            renderer.Models = models;
            renderer.Camera = camera;
            renderer.Lights = new List<PPPointLight>() {
                new PPPointLight(new Vector3(0, 1000, -1000), Color.White * .85f, 20000),
                new PPPointLight(new Vector3(0, 1000, 1000), Color.White * .85f, 20000),
            };
            renderer.ShadowLightPosition = new Vector3(1500, 1500, 2000);
            renderer.ShadowLightTarget = new Vector3(0, 150, 0);
            renderer.DoShadowMapping = true;
            renderer.ShadowMult = 0.3f;

            defaultViewport = GraphicsDevice.Viewport;
            mapViewport.Width = defaultViewport.Width / 4;
            mapViewport.Height = defaultViewport.Height / 4;
            mapViewport.X = 950;
            mapViewport.Y = 10;

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
        MathHelper.PiOver4, 
        16.0f / 9.0f,
        1.0f,
        10000f);
        
            mapProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                2.0f / 2.0f,
                1.0f,
                10000f);

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

            if (keyState.IsKeyDown(Keys.Escape))
            {
                graphics.IsFullScreen = false;
                this.Exit();
            }

            // Move 3 units per millisecond, independent of frame rate
            translation *= 4 *
                (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Move the camera to the new model's position and orientation
            ((ChaseCamera)camera).Move(models[index].Position, models[index].Rotation);
            ((ChaseCameraRadar)cameraTop).Move(models[index].Position, models[index].Rotation);

            // Update the camera
            camera.Update();
            cameraTop.Update();

            // Update the mouse state
            lastMouseState = mouseState;
        }

        // Capsula Lunar 2 posição no Array
        void updateModel(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            Vector3 rotChange = new Vector3(0, 0, 0);

            // Determine on which axes the ship should be rotated on, if any
            if (keyState.IsKeyDown(Keys.S) && models[index].Rotation.X < 0.5f)
                rotChange += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.W) && models[index].Rotation.X > -0.5f)
                rotChange += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.A))
                rotChange += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.D))
                rotChange += new Vector3(0, -1, 0);

            // Posiciona a Capsula no centro do cenário posição Zero
            if (keyState.IsKeyDown(Keys.Z))
            {
                rotChange = new Vector3(0, 0, 0);
                models[index].Rotation = rotChange;
                models[index].Position = LanderDown;
            }

            // Move no eixo Z para avançar
            if (keyState.IsKeyDown(Keys.Up))
            {
                models[index].Position += new Vector3(0, 0, -1) *
                    (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
            }

            // Move no eixo Z para recuar
            if (keyState.IsKeyDown(Keys.Down))
            {
                models[index].Position += new Vector3(0, 0, 1) *
                    (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
            }

            // Move no eixo X para direita
            if (keyState.IsKeyDown(Keys.Right))
            {
                models[index].Position += new Vector3(1, 0, 0) *
                    (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
            }

            // Move no eixo X para esquerda
            if (keyState.IsKeyDown(Keys.Left))
            {
                models[index].Position += new Vector3(-1, 0, 0) *
                    (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
            }

            models[index].Rotation += rotChange * .025f;


            //Physics Update
            shuttleSpeed += new Vector3(0, gravity, 0) * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
            friction = shuttleSpeed * -0.005f;
            shuttleSpeed += friction;

            if (models[index].Position.Y <= 1550)
            {
                models[index].Position = new Vector3(models[index].Position.X, 1550, models[index].Position.Z);
                if (keyState.IsKeyUp(Keys.X) && shuttleSpeed.Y <=0) shuttleSpeed = Vector3.Zero;            
            }

            models[index].Position += shuttleSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;

            // If space isn't down, the ship shouldn't move
            //if (!keyState.IsKeyDown(Keys.Space))
              //  return;

            // Determine what direction to move in
            Matrix rotation = Matrix.CreateFromYawPitchRoll(
                models[index].Rotation.Y, models[index].Rotation.X, models[index].Rotation.Z);

            // Move no eixo Y para subir
            if (keyState.IsKeyDown(Keys.X))
            {
                if (shuttleSpeed.Y < 2f) shuttleSpeed += (Vector3.Transform(new Vector3(0, 0.0001f, 0), rotation) * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4);
                combustivel -= 2.5f;
            }

            // Move in the direction dictated by our rotation matrix
            //models[index].Position += Vector3.Transform(Vector3.Forward, rotation)
              //  * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
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

        protected void UpdateCollision(GameTime gameTime)
        {

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
                        new Vector2(52, 662),
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
                    // adicionar nova camera

                    GraphicsDevice.Viewport = defaultViewport;

                    //spriteBatch.DrawString(arial, "Combustivel: " + (int)combustivel, new Vector2(1025, 0), Color.Yellow);
    
                    sky.Draw(camera.View, camera.Projection, ((ChaseCamera)camera).Position);
                    terrain.Draw(camera.View, camera.Projection);
                    


                    foreach (CModel model in models)
                    {
                        if (camera.BoundingVolumeIsInView(model.BoundingSphere))
                        { 
                            model.Draw(camera.View, camera.Projection, ((ChaseCamera)camera).Position); 
                        }
                        //if (cameraTop.BoundingVolumeIsInView(model.BoundingSphere))
                        //{
                        //    model.Draw(cameraTop.View, cameraTop.Projection, ((ChaseCameraRadar)cameraTop).Position);
                        //}
                        
                    }

                    GraphicsDevice.Viewport = mapViewport;


                    sky.Draw(cameraTop.View, cameraTop.Projection, ((ChaseCamera)camera).Position);
                    terrain.Draw(cameraTop.View, cameraTop.Projection);
                    
                    //spriteBatch.DrawString(arial,
                    //        "Model Position " + models[index].Position +
                    //        "\nModel Rotation: " + models[index].Rotation +
                    //        "\nShuttleSpeedY:" + shuttleSpeed,
                    //        Vector2.Zero,
                    //        Color.Yellow);



                    foreach (CModel model in models)
                    {
                        if (camera.BoundingVolumeIsInView(model.BoundingSphere))
                        {
                            model.Draw(cameraTop.View, cameraTop.Projection, ((ChaseCamera)camera).Position); 
                        }
                        //if (cameraTop.BoundingVolumeIsInView(model.BoundingSphere))
                        //{
                        //    model.Draw(cameraTop.View, cameraTop.Projection, ((ChaseCameraRadar)cameraTop).Position);
                        //}
                        
                    }
                    /*******view port*************/
                    
                    //sky.Draw(cameraTop.View, cameraTop.Projection, ((ChaseCameraRadar)cameraTop).Position);
                    //terrain.Draw(cameraTop.View, cameraTop.Projection); // teste1

                    GraphicsDevice.Viewport = defaultViewport;

                    //spriteBatch.DrawString(arial, "Combustivel: " + (int)combustivel, new Vector2(1025, 0), Color.Yellow);

                    spriteBatch.Draw(mapBorder, new Vector2(940, 0), Color.White); 

                    spriteBatch.DrawString(arial,
                            "Model Position " + models[index].Position +
                            "\nModel Rotation: " + models[index].Rotation +
                            "\nEsc = Exit" +
                            "\nCombustível : " + (int)combustivel,
                            Vector2.Zero,
                            Color.Yellow);

                    break;

                case Screens.INSTRUCTION:
                    spriteBatch.Draw(telaMenu, Vector2.Zero, Color.White);
                    spriteBatch.DrawString(arial,
                        "Lunar controls:" + 
                        "\nKeys A & D Rotation Y" +
                        "\nKeys W & S Rotation X" +
                        "\nKey Z = Return to base" +
                        "\nKey X = Trust" +
                        "\nSpacebar = Trust forward" +
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
