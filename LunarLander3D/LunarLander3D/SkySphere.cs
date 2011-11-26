using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace LunarLander3D
{
    public class SkySphere
    {
        CModel model;
        Effect effect;
        GraphicsDevice graphics;

        /// <summary>
        /// Cria um cubo que recebrá a projeção da textura fornecida
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="GraphicsDevice"></param>
        /// <param name="Texture"></param>
        public SkySphere(ContentManager Content, GraphicsDevice GraphicsDevice, 
            TextureCube Texture)
        {
            model = new CModel(Content.Load<Model>("skysphere_mesh"), Vector3.Zero,
                Vector3.Zero, Vector3.One, GraphicsDevice);

            effect = Content.Load<Effect>("skysphere_effect");
            effect.Parameters["CubeMap"].SetValue(Texture);

            model.SetModelEffect(effect, false);

            this.graphics = GraphicsDevice;
        }

        /// <summary>
        /// Desenha um mapa de textura cubico que será projetado no cubo criado pela classe
        /// </summary>
        /// <param name="View"></param>
        /// <param name="Projection"></param>
        /// <param name="CameraPosition"></param>
        public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        {
            // Disable the depth buffer
            graphics.DepthStencilState = DepthStencilState.None;

            // Move the model with the sphere
            model.Position = CameraPosition;

            model.Draw(View, Projection, CameraPosition);

            graphics.DepthStencilState = DepthStencilState.Default;
        }
    }
}
