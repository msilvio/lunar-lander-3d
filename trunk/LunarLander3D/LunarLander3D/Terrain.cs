﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace LunarLander3D
{
    public class Terrain
    {
        VertexPositionNormalTexture[] vertices; // Vertex array
        VertexBuffer vertexBuffer; // Vertex buffer
        int[] indices; // Index array
        IndexBuffer indexBuffer; // Index buffer
        float[,] heights; // Array of vertex heights
        float height; // Maximum height of terrain
        float cellSize; // Distance between vertices on x and z axes
        int width, length; // Number of vertices on x and z axes
        int nVertices, nIndices; // Number of vertices and indices
        Effect effect; // Effect used for rendering
        GraphicsDevice GraphicsDevice; // Graphics device to draw with
        Texture2D heightMap; // Heightmap texture

        Texture2D baseTexture;
        float textureTiling;
        Vector3 lightDirection;

        public Terrain(Texture2D HeightMap, float CellSize, float Height, 
            Texture2D BaseTexture, float TextureTiling, Vector3 LightDirection, 
            GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            this.baseTexture = BaseTexture;
            this.textureTiling = TextureTiling;
            this.lightDirection = LightDirection;
            this.heightMap = HeightMap;
            this.width = HeightMap.Width;
            this.length = HeightMap.Height;
            this.cellSize = CellSize;
            this.height = Height;

            this.GraphicsDevice = GraphicsDevice;

            effect = Content.Load<Effect>("TerrainEffect");

            // 1 vertex per pixel
            nVertices = width * length;

            // (Width-1) * (Length-1) cells, 2 triangles per cell, 3 indices per triangle
            nIndices = (width - 1) * (length - 1) * 6;

            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture),
                nVertices, BufferUsage.WriteOnly);

            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits,
                nIndices, BufferUsage.WriteOnly);

            getHeights();
            createVertices();
            createIndices();
            genNormals();

            vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);
            indexBuffer.SetData<int>(indices);
        }

        private void getHeights()
        {
            // Extract pixel data
            Color[] heightMapData = new Color[width * length];
            heightMap.GetData<Color>(heightMapData);

            // Create heights[,] array
            heights = new float[width, length];

            // For each pixel
            for (int y = 0; y < length; y++)
                for (int x = 0; x < width; x++)
                {
                    // Get color value (0 - 255)
                    float amt = heightMapData[y * width + x].R;

                    // Scale to (0 - 1)
                    amt /= 255.0f;

                    // Multiply by max height to get final height
                    heights[x, y] = amt * height;
                }
        }

        private void createVertices()
        {
            vertices = new VertexPositionNormalTexture[nVertices];

            // Calculate the position offset that will center the terrain at (0, 0, 0)
            Vector3 offsetToCenter = -new Vector3(((float)width / 2.0f) * cellSize,
                0, ((float)length / 2.0f) * cellSize);

            // For each pixel in the image
            for (int z = 0; z < length; z++)
                for (int x = 0; x < width; x++)
                {
                    // Find position based on grid coordinates and height in heightmap
                    Vector3 position = new Vector3(x * cellSize,
                        heights[x, z], z * cellSize) + offsetToCenter;

                    // UV coordinates range from (0, 0) at grid location (0, 0) to 
                    // (1, 1) at grid location (width, length)
                    Vector2 uv = new Vector2((float)x / width, (float)z / length);

                    // Create the vertex
                    vertices[z * width + x] = new VertexPositionNormalTexture(
                        position, Vector3.Zero, uv);
                }
        }

        private void createIndices()
        {
            indices = new int[nIndices];

            int i = 0;

            // For each cell
            for (int x = 0; x < width - 1; x++)
                for (int z = 0; z < length - 1; z++)
                {
                    // Find the indices of the corners
                    int upperLeft = z * width + x;
                    int upperRight = upperLeft + 1;
                    int lowerLeft = upperLeft + width;
                    int lowerRight = lowerLeft + 1;

                    // Specify upper triangle
                    indices[i++] = upperLeft;
                    indices[i++] = upperRight;
                    indices[i++] = lowerLeft;

                    // Specify lower triangle
                    indices[i++] = lowerLeft;
                    indices[i++] = upperRight;
                    indices[i++] = lowerRight;
                }
        }

        private void genNormals()
        {
            // For each triangle
            for (int i = 0; i < nIndices; i += 3)
            {
                // Find the position of each corner of the triangle
                Vector3 v1 = vertices[indices[i]].Position;
                Vector3 v2 = vertices[indices[i + 1]].Position;
                Vector3 v3 = vertices[indices[i + 2]].Position;

                // Cross the vectors between the corners to get the normal
                Vector3 normal = Vector3.Cross(v1 - v2, v1 - v3);
                normal.Normalize();

                // Add the influence of the normal to each vertex in the
                // triangle
                vertices[indices[i]].Normal += normal;
                vertices[indices[i + 1]].Normal += normal;
                vertices[indices[i + 2]].Normal += normal;
            }

            // Average the influences of the triangles touching each
            // vertex
            for (int i = 0; i < nVertices; i++)
                vertices[i].Normal.Normalize();
        }

        public void Draw(Matrix View, Matrix Projection)
        {
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;

            effect.Parameters["View"].SetValue(View);
            effect.Parameters["Projection"].SetValue(Projection);
            effect.Parameters["BaseTexture"].SetValue(baseTexture);
            effect.Parameters["TextureTiling"].SetValue(textureTiling);
            effect.Parameters["LightDirection"].SetValue(lightDirection);

            effect.Techniques[0].Passes[0].Apply();

            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                nVertices, 0, nIndices / 3);
        }
    }
}
