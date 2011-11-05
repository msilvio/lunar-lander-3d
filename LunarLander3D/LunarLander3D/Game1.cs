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

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

        }

        protected override void Initialize()
        {
            player = new VideoPlayer();
            spriteBatch = new SpriteBatch(this.GraphicsDevice);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            video = Content.Load<Video>("video");
        }

        protected override void UnloadContent()
        {
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
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
