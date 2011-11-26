using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace LunarLander3D
{
    class Lander : CModel
    {
        CModel landerModel;
        float speed;
        Effect effect;

        GraphicsDevice graphicsdevice;
        GameTime gameTime;

        //int modelScale = 50;

        Vector3 LanderDown = new Vector3(500, 4550, -1000); // 500, 2350, -1000
        float gravity = -0.00003f;
        Vector3 shuttleSpeed = Vector3.Zero;
        Vector3 friction = Vector3.Zero;
        public float Combustivel, Oxigenio;

        //public Vector3 Position { get; set; }
        //public Vector3 Rotation { get; set; }
        //public Vector3 Scale { get; set; }

        public Model ModelLander { get; private set; }

        private Matrix[] modelTransforms;
        private GraphicsDevice graphicsDevice;
        private BoundingSphere boundingSphere;

        //public Material Material { get; set; }

        //public float Combustivel, Oxigenio;

        //public BoundingSphere BoundingSphere
        //{
        //    get
        //    {
        //        // No need for rotation, as this is a sphere
        //        Matrix worldTransform = Matrix.CreateScale(Scale)
        //            * Matrix.CreateTranslation(Position);

        //        BoundingSphere transformed = boundingSphere;
        //        transformed = transformed.Transform(worldTransform);

        //        return transformed;
        //    }
        //}

        public Lander(Model Model, Vector3 Position, Vector3 Rotation,
            Vector3 Scale, GraphicsDevice graphicsDevice, float combustivel, float oxigenio) :
            base(Model, Position, Rotation, Scale, graphicsDevice)
        {
            this.ModelLander = Model;

            modelTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelTransforms);

            buildBoundingSphere();
            generateTags();

            this.Position = Position;
            this.Rotation = Rotation;
            this.Scale = Scale;

            this.graphicsDevice = graphicsDevice;

            this.Combustivel = combustivel;
            this.Oxigenio = oxigenio;

            this.Material = new Material();
        }

        private void buildBoundingSphere()
        {
            BoundingSphere sphere = new BoundingSphere(Vector3.Zero, 0);

            // Merge all the model's built in bounding spheres
            foreach (ModelMesh mesh in ModelLander.Meshes)
            {
                BoundingSphere transformed = mesh.BoundingSphere.Transform(
                    modelTransforms[mesh.ParentBone.Index]);

                sphere = BoundingSphere.CreateMerged(sphere, transformed);
            }

            this.boundingSphere = sphere;
        }

        public void Update(GameTime gametime)
        {
            KeyboardState keyState = Keyboard.GetState();
            Vector3 rotChange = new Vector3(0, 0, 0);

            // Determine on which axes the ship should be rotated on, if any
            if (keyState.IsKeyDown(Keys.S) && this.Rotation.X < 0.5f)
                rotChange += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.W) && this.Rotation.X > -0.5f)
                rotChange += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.A))
                rotChange += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.D))
                rotChange += new Vector3(0, -1, 0);

            // Posiciona a Capsula no centro do cenário posição Zero
            if (keyState.IsKeyDown(Keys.Z))
            {
                rotChange = new Vector3(0, 0, 0);
                this.Rotation = rotChange;
                this.Position = LanderDown;
            }

            // Move no eixo Z para avançar
            if (keyState.IsKeyDown(Keys.Up))
            {
                this.Position += new Vector3(0, 0, -1) *
                    (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
            }

            // Move no eixo Z para recuar
            if (keyState.IsKeyDown(Keys.Down))
            {
                this.Position += new Vector3(0, 0, 1) *
                    (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
            }

            // Move no eixo X para direita
            if (keyState.IsKeyDown(Keys.Right))
            {
                this.Position += new Vector3(1, 0, 0) *
                    (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
            }

            // Move no eixo X para esquerda
            if (keyState.IsKeyDown(Keys.Left))
            {
                this.Position += new Vector3(-1, 0, 0) *
                    (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
            }

            this.Rotation += rotChange * .025f;


            //Physics Update
            shuttleSpeed += new Vector3(0, gravity, 0) * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
            friction = shuttleSpeed * -0.005f;
            shuttleSpeed += friction;

            if (this.Position.Y <= 1550)
            {
                this.Position = new Vector3(this.Position.X, 1550, this.Position.Z);
                if (keyState.IsKeyUp(Keys.X) && shuttleSpeed.Y <=0) shuttleSpeed = Vector3.Zero;            
            }

            this.Position += shuttleSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;

            // If space isn't down, the ship shouldn't move
            //if (!keyState.IsKeyDown(Keys.Space))
              //  return;

            // Determine what direction to move in
            Matrix rotation = Matrix.CreateFromYawPitchRoll(
                this.Rotation.Y, this.Rotation.X, this.Rotation.Z);

            // Move no eixo Y para subir
            if (keyState.IsKeyDown(Keys.X))
            {
                if (shuttleSpeed.Y < 2f)
                {
                    shuttleSpeed += (Vector3.Transform(new Vector3(0, 0.0001f, 0), rotation) * 
                        (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4);
                    this.Combustivel -= 2.5f;
                }
            }

            // Move in the direction dictated by our rotation matrix
            //this.Position += Vector3.Transform(Vector3.Forward, rotation)
              //  * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 4;
        }


        //public void Draw(Matrix View, Matrix Projection, Vector3 CameraPosition)
        //{
        //    // Calculate the base transformation by combining
        //    // translation, rotation, and scaling
        //    Matrix baseWorld = Matrix.CreateScale(Scale)
        //        * Matrix.CreateFromYawPitchRoll(
        //            Rotation.Y, Rotation.X, Rotation.Z)
        //        * Matrix.CreateTranslation(Position);

        //    foreach (ModelMesh mesh in ModelLander.Meshes)
        //    {
        //        Matrix localWorld = modelTransforms[mesh.ParentBone.Index]
        //            * baseWorld;

        //        foreach (ModelMeshPart meshPart in mesh.MeshParts)
        //        {
        //            Effect effect = meshPart.Effect;

        //            if (effect is BasicEffect)
        //            {
        //                ((BasicEffect)effect).World = localWorld;
        //                ((BasicEffect)effect).View = View;
        //                ((BasicEffect)effect).Projection = Projection;
        //                ((BasicEffect)effect).EnableDefaultLighting();
        //            }
        //            else
        //            {
        //                setEffectParameter(effect, "World", localWorld);
        //                setEffectParameter(effect, "View", View);
        //                setEffectParameter(effect, "Projection", Projection);
        //                setEffectParameter(effect, "CameraPosition", CameraPosition);

        //                Material.SetEffectParameters(effect);
        //            }
        //        }

        //        mesh.Draw();
        //    }
        //}

        // Sets the specified effect parameter to the given effect, if it
        // has that parameter
        void setEffectParameter(Effect effect, string paramName, object val)
        {
            if (effect.Parameters[paramName] == null)
                return;

            if (val is Vector3)
                effect.Parameters[paramName].SetValue((Vector3)val);
            else if (val is bool)
                effect.Parameters[paramName].SetValue((bool)val);
            else if (val is Matrix)
                effect.Parameters[paramName].SetValue((Matrix)val);
            else if (val is Texture2D)
                effect.Parameters[paramName].SetValue((Texture2D)val);
        }

        private void generateTags()
        {
            foreach (ModelMesh mesh in ModelLander.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    if (part.Effect is BasicEffect)
                    {
                        BasicEffect effect = (BasicEffect)part.Effect;
                        MeshTag tag = new MeshTag(effect.DiffuseColor,
                            effect.Texture, effect.SpecularPower);
                        part.Tag = tag;
                    }
        }




    }

}
