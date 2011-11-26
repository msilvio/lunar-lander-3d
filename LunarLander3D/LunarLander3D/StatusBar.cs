using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace LunarLander3D
{
    class StatusBar
    {
        public int tamanho;
        Texture2D mHealthBar;
        Viewport viewport;
        Vector2 Position;

        public int Healths{
            get { return tamanho; }
            set {tamanho = value; }
        }

        public StatusBar(Texture2D mhealthBar, Viewport viewport, Vector2 position, int max) 
        {
            this.mHealthBar = mhealthBar;
            this.viewport = viewport;
            this.Position = position;
            this.tamanho = max;
        }

        public void Init() {
        }

        public void Update(GameTime gameTime) 
        {
            KeyboardState mKeys = Keyboard.GetState();
            if (mKeys.IsKeyDown(Keys.Up) == true)
            {
                tamanho += 1;
            }

            if (mKeys.IsKeyDown(Keys.Down) == true)
            {
                tamanho -= 1;
            }

            tamanho = (int)MathHelper.Clamp(tamanho, 0, 100);

            //this.Position = position;
            
        }

        public void Draw(SpriteBatch spriteBatch) 
        {

            spriteBatch.Draw(mHealthBar,
                            new Rectangle((int)this.Position.X, // ajustado a largura
                                          (int)this.Position.Y, // ajustado a altura
                                          (tamanho * mHealthBar.Width / 2) / 100, // reduzida metade do tamanho
                                           mHealthBar.Height),
                                           Color.White);

            //spriteBatch.Draw(mHealthBar,
            //    new Rectangle((int)this.Position.X, // ajustado a largura
            //                  (int)this.Position.Y, // ajustado a altura
            //                  mHealthBar.Width, // reduzida metade do tamanho
            //                  (tamanho * mHealthBar.Height / 2) / 100,
            //                   Color.White);

        }
    }
}
