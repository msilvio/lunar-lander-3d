using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace LunarLander3D
{
    class Menu
    {
        public enum Selection { START, OPTIONS, EXIT, NONE, CREDITS }
        public Texture2D arrowTexture;
        //public Texture2D background;

        int menuStartY = 350;
        int menuStartX = 515;
        int arrowSelectionIndex;
        Rectangle arrowPosition;
        public Selection arrowSelection;
        public Selection Selected = Selection.NONE;
        public string[] strings = new string[4];
        //GamePadState gamePadStateprev;

        public void Initialize(ContentManager content)
        {
            //background = content.Load<Texture2D>("Graphics/logo_screen");
            arrowTexture = content.Load<Texture2D>("Graphics/arrow_right");

            //START
            strings[0] = "NEW GAME";

            //OPTIONS
            strings[1] = "INSTRUCTIONS";

            //EXIT
            strings[2] = "EXIT";

            //CREDITS
            strings[3] = "CREDITS";
        }

        public void Update(KeyboardState keyboardState, KeyboardState previousState, GamePadState gamePadState, GamePadState gamePadStateprev)
        {
            // Controles do menu pelo Gamepad
            //gamePadStateprev = GamePad.GetState(PlayerIndex.One);

            //if (gamePadState.Buttons.Back == ButtonState.Pressed)
            //{
            //    Selected = Selection.EXIT;
            //}

            //if ((gamePadState.Buttons.Start == ButtonState.Pressed) && 
            //    (gamePadStateprev.Buttons.Start == ButtonState.Released))
            //{ 
            //    Selected = Selection.START; 
            //}

            switch (arrowSelection)
            {
                case Selection.START: 
                    if ((keyboardState.IsKeyDown(Keys.Enter) && (previousState.IsKeyUp(Keys.Enter))) ||
                        ((gamePadState.Buttons.Start == ButtonState.Pressed) && 
                        !(gamePadStateprev.Buttons.Start == ButtonState.Pressed)))
                        { Selected = Selection.START; }
                    break;
                case Selection.OPTIONS:
                    if ((keyboardState.IsKeyDown(Keys.Enter) && (previousState.IsKeyUp(Keys.Enter))) ||
                        ((gamePadState.Buttons.Start == ButtonState.Pressed) &&
                        !(gamePadStateprev.Buttons.Start == ButtonState.Pressed))) 
                        { Selected = Selection.OPTIONS; }
                    break;
                case Selection.EXIT:
                    if ((keyboardState.IsKeyDown(Keys.Enter) && (previousState.IsKeyUp(Keys.Enter))) ||
                        ((gamePadState.Buttons.Start == ButtonState.Pressed) &&
                        !(gamePadStateprev.Buttons.Start == ButtonState.Pressed))) 
                        { Selected = Selection.EXIT; }
                    break;
                case Selection.CREDITS:
                    if ((keyboardState.IsKeyDown(Keys.Enter) && (previousState.IsKeyUp(Keys.Enter))) ||
                        ((gamePadState.Buttons.Start == ButtonState.Pressed) &&
                        !(gamePadStateprev.Buttons.Start == ButtonState.Pressed))) 
                        { Selected = Selection.CREDITS; }
                    break;

            }


            if (keyboardState.IsKeyDown(Keys.Down) && (previousState.IsKeyUp(Keys.Down)) || 
                gamePadState.DPad.Down == ButtonState.Pressed && gamePadStateprev.DPad.Down == ButtonState.Released)
            {
                if (arrowSelectionIndex != strings.Count() - 1)
                    arrowSelectionIndex++;
                else arrowSelectionIndex = 0;
            }

            if (keyboardState.IsKeyDown(Keys.Up) && (previousState.IsKeyUp(Keys.Up)) ||
                gamePadState.DPad.Up == ButtonState.Pressed && gamePadStateprev.DPad.Up == ButtonState.Released)
            {
                if (arrowSelectionIndex != 0)
                    arrowSelectionIndex--;
                else arrowSelectionIndex = strings.Count() - 1;
            }
            arrowPosition = new Rectangle(menuStartX - 50, menuStartY - 5 + (40 * arrowSelectionIndex), 40, 40);


            switch (arrowSelectionIndex)
            {
                case 0:
                    arrowSelection = Selection.START;
                    break;
                case 1:
                    arrowSelection = Selection.OPTIONS;
                    break;
                case 2:
                    arrowSelection = Selection.EXIT;
                    break;
                case 3:
                    arrowSelection = Selection.CREDITS;
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont spriteFont) 
        {
            //spriteBatch.Draw(background, Vector2.Zero, Color.White);

            for (int i = 0; i < strings.Count(); i++)
            {
                spriteBatch.DrawString(spriteFont, strings[i], new Vector2(menuStartX + 2, menuStartY + (i * 40) + 2), Color.Black);
                spriteBatch.DrawString(spriteFont, strings[i], new Vector2(menuStartX, menuStartY + (i * 40)), Color.Yellow); 
            }
            spriteBatch.Draw(arrowTexture, arrowPosition, Color.White);
        }
    }
}

