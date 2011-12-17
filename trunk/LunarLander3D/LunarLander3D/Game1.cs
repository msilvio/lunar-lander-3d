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
    /// <summary>
    /// 
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        float combustivel = 4000;
        float oxigenio = 5000;
        int conta1 = 100;
        int conta2 = 100;

        int[] scorelist = new int[10];

        Texture2D RedBarImg, GreenBarImg;
        StatusBar RedBar, GreenBar;

        VideoPlayer player;
        Video video, video1;
        Song gameplayMusic;
        SoundEffect rocketSound, explosionSound;
        Texture2D videoTexture;
        Texture2D mapBorder, mapBorder2;
        Save saveGame = new Save();
        bool played;
        bool shipGrounded;

        private float titleScreenTimer = 0.0f;
        private float titleScreenDelayTime = 1.0f;

        //Particulas

        ParticleSystem explosionParticle;
        ParticleSystem explosionSmokeParticle;
        ParticleSystem smokeParticle;
        ParticleSystem fireParticle;
        ParticleSystem projectileParticle;

        List<Projectile> projectiles;
        TimeSpan timeToNextProjectile = TimeSpan.Zero;

        enum ParticleType { SMOKE, EXPLOSION, FIRE };
        ParticleType particleType = ParticleType.FIRE;

        /// <summary>
        /// Enum utilizado para enumerar as telas do jogo
        /// </summary>
        enum Screens { INTRO, MENU, GAME, INSTRUCTION, GAMEOVER, CREDITS }; 
     
        /// <summary>
       /// Defini��o de qual tela come�ar� o jogo
       /// </summary>
        Screens currentScreen = Screens.INTRO;

        KeyboardState keyState, oldKeyState, previousState;
        GamePadState gamepadState, gamePadStateprev;

        /// <summary>
        /// SriteFont utilizado para representar a fonte do texto
        /// </summary>
        SpriteFont arial;

        /// <summary>
        /// Textura da tela de menu do jogo
        /// </summary>
        Texture2D telaMenu, telaMenuInst;

        /// <summary>
        /// Carregamento da classe menu
        /// </summary>
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
        // Parametro para alterar entre camera principal e secundaria
        bool mainCam = true;
        // Parametro para exibir ou n�o displays secund�rios
        bool hud = true;
        bool hudprev = true;

        /// <summary>
        /// Carregamento da classe SkySphere que desenha o mapa no formato de um esfera
        /// </summary>
        SkySphere sky;

        // Posi��o inicial da camera  - 0, 600, 1500  // 8000, 6000, 8000 // 0, 400, 1200
        Vector3 cameraPos = new Vector3(100, 450, 1200);
        Vector3 cameraPosTop = new Vector3(0, 1450, 0);

        PrelightingRenderer renderer;

        MouseState lastMouseState;

        /// <summary>
        /// viewport padrao, deve mostrar a cena toda por tr�s do m�dulo lunar...
        /// </summary>
        Viewport defaultViewport;

        /// <summary>
        /// viewport do mapa, deve mostrar a cena toda por cima do m�dulo lunar...
        /// </summary>
        Viewport mapViewport;
        
        /// <summary>
        /// proje��o da viewport padrao
        /// </summary>
        Matrix projectionMatrix;

        ///// <summary>
        ///// proje��o da viewport do mapa
        ///// </summary>
        //Matrix mapProjectionMatrix;

        /// <summary>
        /// 
        /// </summary>
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

            //Carrega Part�culas
            explosionParticle = new ExplosionParticleSystem(this, this.Content);
            fireParticle = new FireParticleSystem(this, Content);
            smokeParticle = new SmokePlumeParticleSystem(this, Content);
            explosionSmokeParticle = new ExplosionSmokeParticleSystem(this, Content);
            projectileParticle = new ProjectileTrailParticleSystem(this, Content);

            //Define Prioridade dos efeitos de particulas;
            explosionParticle.DrawOrder = 100;
            explosionSmokeParticle.DrawOrder = 200;
            fireParticle.DrawOrder = 300;
            smokeParticle.DrawOrder = 400;
            projectileParticle.DrawOrder = 500;

            Components.Add(explosionParticle);
            Components.Add(smokeParticle);
            Components.Add(fireParticle);
            Components.Add(projectileParticle);
            Components.Add(explosionSmokeParticle);

        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Initialize()
        {
            
            player = new VideoPlayer();
            spriteBatch = new SpriteBatch(this.GraphicsDevice);
            menu.Initialize(this.Content);
            base.Initialize();
        }


        protected override void LoadContent()
        {
            
            
            
            //Carrega V�deo Inicial
            video = Content.Load<Video>("Videos/Lunar3D_Show");
            video1 = Content.Load<Video>("Videos/Lunar_menu");
            player = new VideoPlayer();

            //Carrega Barras de Oxigenio/Combustivel
            RedBarImg = Content.Load<Texture2D>("Graphics/Bar");
            GreenBarImg = Content.Load<Texture2D>("Graphics/GreenBar");

            RedBar = new StatusBar(RedBarImg, GraphicsDevice.Viewport, new Vector2(200, 110), (int)oxigenio);
            GreenBar = new StatusBar(GreenBarImg, GraphicsDevice.Viewport, new Vector2(200, 150), (int)combustivel);

            // Audio
            gameplayMusic = Content.Load<Song>("Sounds/Blake-Nowhere-near");
            rocketSound = Content.Load<SoundEffect>("Sounds/laserFire");
            explosionSound = Content.Load<SoundEffect>("Sounds/explosion");

                    
            arial = Content.Load<SpriteFont>("arial");

            telaMenu = Content.Load<Texture2D>("Graphics/logo_screen");

            telaMenuInst = Content.Load<Texture2D>("Graphics/logo_screen_menu");

            mapBorder = Content.Load<Texture2D>("Graphics/border1");
            mapBorder2 = Content.Load<Texture2D>("Graphics/border");

            //models.Add(new CModel(Content.Load<Model>("ground"),
               //new Vector3(0, -2000, 0), Vector3.Zero, new Vector3(1, 1, 1), GraphicsDevice));

            // 30, 4800 - 30, 9600 - 30, 3200 - 30, 256 (terrains 4 e 5)
            terrain = new Terrain(Content.Load<Texture2D>("Graphics/terrain3"), 30, 3200,
                Content.Load<Texture2D>("Graphics/terrain"), 6, new Vector3(1, -1, 0), // 6
                GraphicsDevice, Content);

            // Capsula Lunar 2 posi��o no Array - colocar o index correto - index = 2 / Agora index =1
            index = 0;
            models.Add(new CModel(Content.Load<Model>("modulo1"),
                LanderDown, Vector3.Zero, 
                new Vector3(modelScale, modelScale, modelScale), 
                GraphicsDevice));

            //models.Add(new Lander(Content.Load<Model>("modulo1"),
            //    LanderDown, Vector3.Zero,
            //    new Vector3(modelScale, modelScale, modelScale),
            //    GraphicsDevice,
            //    combustivel,
            //    oxigenio));

            Effect lightingEffect = Content.Load<Effect>("LightingEffect");
            LightingMaterial lightingMat = new LightingMaterial();

            Effect normalMapEffect = Content.Load<Effect>("NormalMapEffect");
            //NormalMapMaterial normalMat = new NormalMapMaterial(
            //    Content.Load<Texture2D>("brick_normal_map"));
            
            //lightingMat.LightDirection = new Vector3(.5f, .5f, 1); // posi��o anterior da luz
            lightingMat.LightDirection = new Vector3(-1.5f, .8f, 1);
            lightingMat.LightColor = Vector3.One;

            Effect effect = Content.Load<Effect>("VSM");

            //normalMat.LightDirection = new Vector3(.5f, .5f, 1);
            //normalMat.LightColor = Vector3.One;

            //models[0].SetModelEffect(lightingEffect, true);
            //models[1].SetModelEffect(normalMapEffect, true);

            //models[0].Material = lightingMat;
            //models[1].Material = normalMat;

            // Antes do Shadow
            camera = new ChaseCamera(cameraPos, new Vector3(0, 200, 0), // 0, 200, 0
                new Vector3(0, 0, 0), GraphicsDevice);

            cameraTop = new ChaseCameraRadar(cameraPosTop, new Vector3(0, 0, 0), 
                new Vector3(0, 0, 0), GraphicsDevice);

            sky = new SkySphere(Content, GraphicsDevice,
                Content.Load<TextureCube>("Graphics/Black_sky"));

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

            // Defini��o do Viewport principal e secund�rio
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
        
            //mapProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            //    MathHelper.PiOver4,
            //    2.0f / 2.0f,
            //    1.0f,
            //    10000f);

            lastMouseState = Mouse.GetState();

            scorelist = saveGame.LoadScore();
            saveGame.SaveScore(scorelist);
            saveGame.SaveGame(LanderDown, combustivel, oxigenio);

        }

        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>

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

        #region UpdateParticleEffects
        /// <summary>
        /// Helper for updating the explosions effect.
        /// </summary>
        void UpdateExplosions(GameTime gameTime)
        {
            timeToNextProjectile -= gameTime.ElapsedGameTime;

            if (timeToNextProjectile <= TimeSpan.Zero)
            {
                // Create a new projectile once per second. The real work of moving
                // and creating particles is handled inside the Projectile class.
                projectiles.Add(new Projectile(explosionParticle,
                                               explosionSmokeParticle,
                                               projectileParticle));

                timeToNextProjectile += TimeSpan.FromSeconds(1);
            }
        }


        /// <summary>
        /// Helper for updating the list of active projectiles.
        /// </summary>
        void UpdateProjectiles(GameTime gameTime)
        {
            int i = 0;

            while (i < projectiles.Count)
            {
                if (!projectiles[i].Update(gameTime))
                {
                    // Remove projectiles at the end of their life.
                    projectiles.RemoveAt(i);
                }
                else
                {
                    // Advance to the next projectile.
                    i++;
                }
            }
        }


        /// <summary>
        /// Helper for updating the smoke plume effect.
        /// </summary>
        void UpdateSmokePlume()
        {
            // This is trivial: we just create one new smoke particle per frame.
            smokeParticle.AddParticle(Vector3.Zero, Vector3.Zero);
        }


        /// <summary>
        /// Helper for updating the fire effect.
        /// </summary>
        void UpdateFire()
        {

            const int fireParticlesPerFrame = 25;

            // Create a number of fire particles, randomly positioned around an area.
            for (int i = 0; i < fireParticlesPerFrame; i++)
            {
                fireParticle.AddParticle(randomizeParticle(), Vector3.Zero);
            }

            // Create one smoke particle per frmae, too.
            smokeParticle.AddParticle(randomizeParticle(), Vector3.Zero);
            
        }

        Vector3 randomizeParticle()
        {
            Random random = new Random();
            Vector3 vec = new Vector3(random.Next((int)models[index].Position.X - 25,
                                      (int)models[index].Position.X + 25), 
                                      random.Next((int)models[index].Position.Y - 25, 
                                      (int)models[index].Position.Y), 
                                      models[index].Position.Z);
            //vec = models[index].Rotation * vec;
            return vec;
        }

        #endregion

        private static Vector3 handleGamePadSlide(GamePadState gamepadState)
        {
            return new Vector3(
                gamepadState.ThumbSticks.Right.X, 0,
                -gamepadState.ThumbSticks.Right.Y);
        }

        private static Vector3 handleGamePadMovement(GamePadState gamepadState)
        {
            return new Vector3(
                gamepadState.ThumbSticks.Left.Y,
                -gamepadState.ThumbSticks.Left.X,0);
        }

        private static Vector3 handleGamePadTrust(GamePadState gamepadState)
        {
            return new Vector3(
                0,
                gamepadState.Triggers.Left / 100, 0);

        }

        // Capsula Lunar [index] define posi��o no Array
        void updateModel(GameTime gameTime, 
                        KeyboardState keyState, 
                        KeyboardState previousState, 
                        GamePadState gamepadState, 
                        GamePadState gamePadStateprev)
        {
            //KeyboardState keyState = Keyboard.GetState();

            //KeyboardState OldKeyState = keyState;

            Vector3 rotChange = new Vector3(0, 0, 0);

            // incluido para controlar a execu��o inicial
            titleScreenTimer +=
                (float)gameTime.ElapsedGameTime.TotalSeconds;

            conta1 = (int)(oxigenio / 50);
            RedBar.tamanho = conta1;
            oxigenio -= 1f;

            //RedBar.tamanho = (int)oxigenio;
            RedBar.Update(gameTime);
            GreenBar.Update(gameTime);
            if ((oxigenio <= 0) || (combustivel <=0))
            {
                //explosionSound.Play(); // tirar o comentario dessa linha ap�s os testes
                //currentScreen = Screens.GAMEOVER; // tirar o comentario dessa linha ap�s os testes
                //MediaPlayer.Stop(); // tirar o comentario dessa linha ap�s os testes
                saveGame.SaveScore(scorelist);
            }

            // movimentos de rota��o eixos X e Y pelo GamePad
            rotChange +=
                handleGamePadMovement(GamePad.GetState(PlayerIndex.One));

            // movimentos de deslocamentos nos eixos X e Z pelo GamePad
            models[index].Position +=
                handleGamePadSlide(GamePad.GetState(PlayerIndex.One)) *
                    (float)gameTime.ElapsedGameTime.TotalMilliseconds * 2;


            // Determine on which axes the ship should be rotated on, if any
            if (keyState.IsKeyDown(Keys.S) && models[index].Rotation.X < 0.5f)
                rotChange += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.W) && models[index].Rotation.X > -0.5f)
                rotChange += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.A))
                rotChange += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.D))
                rotChange += new Vector3(0, -1, 0);

            // Posiciona a Capsula no centro do cen�rio posi��o Zero
            if (keyState.IsKeyDown(Keys.Z) ||
                (GamePad.GetState(PlayerIndex.One).Buttons.RightShoulder == ButtonState.Pressed)) 
            {
                rotChange = new Vector3(0, 0, 0);
                models[index].Rotation = rotChange;
                models[index].Position = LanderDown;
            }

            // Exibe ou n�o as telas secund�rias
            if ((keyState.IsKeyDown(Keys.H) && !(previousState.IsKeyDown(Keys.H))) ||
                    (gamepadState.Buttons.A == ButtonState.Pressed) &&
                    !(gamePadStateprev.Buttons.A == ButtonState.Pressed))
                    
            {
                hud = !hud;
                //Console.WriteLine(keyState.IsKeyDown(Keys.H));
                //Console.WriteLine(OldKeyState.IsKeyDown(Keys.H));
            }

            // Controle de troca de camera entre os viewports
            if ((keyState.IsKeyDown(Keys.T) && !(previousState.IsKeyDown(Keys.T))) ||
                    (gamepadState.Buttons.B == ButtonState.Pressed) &&
                    !(gamePadStateprev.Buttons.B == ButtonState.Pressed))
            {
                mainCam = !mainCam;
            }

            // Move no eixo Z para avan�ar
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

            if (models[index].Position.Y <= 1550 || shipGrounded)
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
            if (keyState.IsKeyDown(Keys.X) ||
                (GamePad.GetState(PlayerIndex.One).Buttons.LeftShoulder == ButtonState.Pressed))
            {
                UpdateFire();
                if (shuttleSpeed.Y < 2f)
                {
                    shuttleSpeed += (Vector3.Transform(new Vector3(0, 0.0001f, 0), rotation) * 
                        (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4);
                    conta2 = (int)(combustivel / 40);
                    GreenBar.tamanho = conta2;
                    combustivel -= 2.5f;
                    
                    // rocketSound.Play(); // substituir o som de laser pelo som de foguete
                }
            }
           

            //OldKeyState = keyState;
        }

        protected override void Update(GameTime gameTime)
        {

            // incluido para controlar a execu��o inicial
            titleScreenTimer +=
                (float)gameTime.ElapsedGameTime.TotalSeconds;

            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            //    this.Exit();

            keyState = Keyboard.GetState();

            gamepadState = GamePad.GetState(PlayerIndex.One);

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
                        (Keyboard.GetState().IsKeyDown(Keys.Escape)) ||
                            (GamePad.GetState(PlayerIndex.One).Buttons.Start ==
                            ButtonState.Pressed))
                    {
                        player.Stop();
                        currentScreen = Screens.MENU;
                        player.IsLooped = true;
                        player.Play(video1);
                    }
                    videoTexture = player.GetTexture();
                    break;

                case Screens.MENU:

                    menu.Update(keyState, previousState,
                                gamepadState, gamePadStateprev);

                    if (player.State == MediaState.Stopped)
                    {
                        played = true;
                        player.Play(video1);
                        videoTexture = player.GetTexture(); 
                    }

                    if (((played) && (player.State == MediaState.Stopped)) ||
                        ((keyState.IsKeyDown(Keys.Enter) && (previousState.IsKeyUp(Keys.Enter))) ||
                        ((gamepadState.Buttons.Start == ButtonState.Pressed) && 
                        !(gamePadStateprev.Buttons.Start == ButtonState.Pressed))))
                         
                    {
                        player.Stop();
                        if (titleScreenTimer >= titleScreenDelayTime)
                        {
                            PlayMusic(gameplayMusic);
                        }

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
                            case Menu.Selection.CREDITS:
                                currentScreen = Screens.CREDITS;
                                menu.Selected = Menu.Selection.NONE;
                                break;
                        }
                    }
                    videoTexture = player.GetTexture();
                    break;

                case Screens.GAME:

                    if ((keyState.IsKeyDown(Keys.Escape) && (previousState.IsKeyUp(Keys.Escape))) ||
                        ((gamepadState.Buttons.Back == ButtonState.Pressed) &&
                        !(gamePadStateprev.Buttons.Back == ButtonState.Pressed)))
                        { this.Exit(); }

                    //MouseState mouseState = Mouse.GetState();

                    // Determine how much the camera should turn
                    //float deltaX = (float)lastMouseState.X - (float)mouseState.X;
                    //float deltaY = (float)lastMouseState.Y - (float)mouseState.Y;

                    Vector3 translation = Vector3.Zero;

                    if (keyState.IsKeyDown(Keys.Escape)) this.Exit();

                    // Move 3 units per millisecond, independent of frame rate
                    translation *= 4 *
                        (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    // Update the camera
                    updateModel(gameTime, keyState, previousState, gamepadState, gamePadStateprev);
                    updateCamera(gameTime);
                    
                    //Update Particles
                    //UpdateParticles(gameTime);

                    //RedBar.Update(gameTime, Vector2.Zero);

                    // Update the mouse state
                    //lastMouseState = mouseState;
                    break;

                case Screens.INSTRUCTION:
                    if ((keyState.IsKeyDown(Keys.Escape) && (previousState.IsKeyUp(Keys.Escape))) ||
                        ((gamepadState.Buttons.Back == ButtonState.Pressed) && 
                        !(gamePadStateprev.Buttons.Back == ButtonState.Pressed)))
                    {
                        currentScreen = Screens.MENU;
                        menu.Selected = Menu.Selection.NONE;
                    }
                    break;

                case Screens.CREDITS:
                    if ((keyState.IsKeyDown(Keys.Escape) && (previousState.IsKeyUp(Keys.Escape))) ||
                        ((gamepadState.Buttons.Back == ButtonState.Pressed) &&
                        !(gamePadStateprev.Buttons.Back == ButtonState.Pressed)))
                    {
                        currentScreen = Screens.MENU;
                        menu.Selected = Menu.Selection.NONE;
                    }
                    break;

                case Screens.GAMEOVER:
                    if ((keyState.IsKeyDown(Keys.Escape) && (previousState.IsKeyUp(Keys.Escape))) ||
                        ((gamepadState.Buttons.Back == ButtonState.Pressed) &&
                        !(gamePadStateprev.Buttons.Back == ButtonState.Pressed))) 
                        { this.Exit(); }
                    break;
            }


            //previousState = Keyboard.GetState();

            previousState = keyState;

            gamePadStateprev = gamepadState;
            //oldKeyState = keyState;
            base.Update(gameTime);
        }


        /// <summary>
        /// Fun��o para executar arquivos MP3
        /// </summary>
        /// <param name="song"></param>

        private void PlayMusic(Song song)
        {
            try
            {
                MediaPlayer.Play(song);
                MediaPlayer.IsRepeating = true;
            }
            catch { }
        }

        /// <summary>
        /// Fun��o para definir parametros de camera para vis�o principal
        /// </summary>
        private void mainViewPort()
        {
            GraphicsDevice.Viewport = defaultViewport;

            sky.Draw(camera.View, camera.Projection, ((ChaseCamera)camera).Position);
            terrain.Draw(camera.View, camera.Projection);

            foreach (CModel model in models)
            {
                if (camera.BoundingVolumeIsInView(model.BoundingSphere))
                {
                    model.Draw(camera.View, camera.Projection, ((ChaseCamera)camera).Position);
                }
            }
        }

        /// <summary>
        /// Fun��o para definir parametros de camera para vis�o principal invertida
        /// </summary>
        private void mainViewPortChange()
        {
            GraphicsDevice.Viewport = defaultViewport;

            sky.Draw(cameraTop.View, cameraTop.Projection, ((ChaseCameraRadar)cameraTop).Position);
            terrain.Draw(cameraTop.View, cameraTop.Projection);

            foreach (CModel model in models)
            {
                if (camera.BoundingVolumeIsInView(model.BoundingSphere))
                {
                    model.Draw(cameraTop.View, cameraTop.Projection, ((ChaseCameraRadar)cameraTop).Position);
                }
            }
        }

        /// <summary>
        /// Fun��o para definir parametros de camera para vis�o secundaria
        /// </summary>
        private void childViewPort()
        {
            GraphicsDevice.Viewport = mapViewport;

            sky.Draw(cameraTop.View, cameraTop.Projection, ((ChaseCameraRadar)cameraTop).Position);
            terrain.Draw(cameraTop.View, cameraTop.Projection);

            foreach (CModel model in models)
            {
                if (camera.BoundingVolumeIsInView(model.BoundingSphere))
                {
                    model.Draw(cameraTop.View, cameraTop.Projection, ((ChaseCameraRadar)cameraTop).Position);
                }
            }
        }

        /// <summary>
        /// Fun��o para definir parametros de camera para vis�o secundaria invertida
        /// </summary>
        private void childViewPortChange()
        {
            GraphicsDevice.Viewport = mapViewport;

            sky.Draw(camera.View, camera.Projection, ((ChaseCamera)camera).Position);
            terrain.Draw(camera.View, camera.Projection);

            foreach (CModel model in models)
            {
                if (camera.BoundingVolumeIsInView(model.BoundingSphere))
                {
                    model.Draw(camera.View, camera.Projection, ((ChaseCamera)camera).Position);
                }
            }
        }

        protected void UpdateCollision(GameTime gameTime)
        {
            
        }

        protected void UpdateParticles(GameTime gameTime)
        {
            switch (particleType)
            {
                case ParticleType.FIRE:
                    UpdateFire();
                    break;
                case ParticleType.EXPLOSION:
                    UpdateExplosions(gameTime);
                    break;
                case ParticleType.SMOKE:
                    UpdateSmokePlume();
                    break;
            }
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

                    spriteBatch.DrawString(arial,
                        "currentScreen: " + currentScreen +
                        "\nmenu.Selected: " + menu.Selected,
                        Vector2.Zero,
                        Color.Yellow);
                    break;

                case Screens.GAME:

                    //Desenhar Part�culas
                    explosionParticle.SetCamera(camera.View, camera.Projection);
                    explosionSmokeParticle.SetCamera(camera.View, camera.Projection);
                    projectileParticle.SetCamera(camera.View, camera.Projection);
                    smokeParticle.SetCamera(camera.View, camera.Projection);
                    fireParticle.SetCamera(camera.View, camera.Projection);

                    // Teste troca de Viewports
                    if (mainCam)
                    {
                        // ViewPort Principal
                        mainViewPort();
                        // ViewPort Secund�rio
                        if (hud) childViewPort();
                    }
                    else
                    {
                        // ViewPort Principal
                        mainViewPortChange();
                        // ViewPort Secund�rio
                        if (hud) childViewPortChange();
                    }

                    /*******view port reload************/
                    GraphicsDevice.Viewport = defaultViewport;

                    // Testa exibir displays
                    if (hud)
                    {
                        spriteBatch.Draw(mapBorder, new Vector2(940, 0), Color.White);
                        spriteBatch.Draw(mapBorder2, new Vector2(0, 0), Color.White);
                        RedBar.Draw(spriteBatch);
                        GreenBar.Draw(spriteBatch);
                        spriteBatch.DrawString(arial,
                                "Position: " + "\n" + models[index].Position +
                                "\nRotation: " + "\n" + models[index].Rotation +
                                "\nOxigene:  " + oxigenio +
                                "\n " +
                                "\nFuel       : " + combustivel,
                                new Vector2(18, 9),
                                Color.Yellow);
                    }

                    // Retirar apos testes
                    spriteBatch.DrawString(arial,
                        "titleScreenTimer " + titleScreenTimer +
                        "\ntitleScreenDelayTime "  + titleScreenDelayTime,
                        new Vector2(480, 9),
                        Color.Yellow);

                    break;

                case Screens.INSTRUCTION:
                    spriteBatch.Draw(telaMenuInst, Vector2.Zero, Color.White);
                    spriteBatch.DrawString(arial,
                        "Lunar controls:" + 
                        "\nKeys A & D Rotation Y" +
                        "\nKeys W & S Rotation X" +
                        "\nKey T = Change camera view" +
                        "\nKey Z = Return to base" +
                        "\nKey X = Trust" +
                        "\nSpacebar = Trust forward" +
                        "\nEscape = Return to Menu or" +
                        "\n                   Exit in Game",
                        new Vector2(100, 300),
                        Color.Yellow);
                     
                    break;

                case Screens.CREDITS:
                    spriteBatch.Draw(telaMenu, Vector2.Zero, Color.White);
                    spriteBatch.DrawString(arial,
                        "Lunar Lander 3D credits:" +
                        "\n" +
                        "\nSilvio Mendonca - Programmer" +
                        "\nMarcos Thiago - Programmer" +
                        "\nRodrigo Raiser - Programmer" +
                        "\n" +
                        "\nEscape = Return to Menu",
                        new Vector2(100, 300),
                        Color.Yellow);

                    break;

                case Screens.GAMEOVER:
                    spriteBatch.Draw(telaMenu, Vector2.Zero, Color.White);
                    spriteBatch.DrawString(arial,
                        "Press Escape to exit",
                        new Vector2(51, 661),
                        Color.Black);
                    spriteBatch.DrawString(arial,
                        "Press Escape to exit",
                        new Vector2(51, 661),
                        Color.Yellow);
                    foreach (int i in scorelist)
                    {
                        spriteBatch.DrawString(arial, "Score: " + i, new Vector2(300, 600), Color.Yellow);
                    }
                    break;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
