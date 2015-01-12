using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity_Editor
{
    public class Terrain
    {
        public bool Loaded = false;

        public CustomVertex.PositionNormalColored[] Vertices;
        private short[] Indices;

        public int[,] HeightData;
        public Size Size;
        public Material Material;
        public Texture Tile;

        public Terrain(Device Device, string Path)
        {
            LoadHeightData(Path);
            VertexDeclaration(Device);
            IndicesDeclaration(Device);
            GenerateNormals();

            Material.Ambient = Color.FromArgb(0, 0, 0, 0);
            Material.Diffuse = Color.FromArgb(0, 0, 0, 0);
            Material.Specular = Color.FromArgb(0, 0, 0, 0);
            Material.Emissive = Color.FromArgb(0, 0, 0, 0);

            Tile = TextureLoader.FromFile(Device, Path + @"\Tile.png");

            Loaded = true;
        }

        private void LoadHeightData(string Path)
        {
            int offset;
            byte dummy;

            FileStream fs = new FileStream(Path + @"\base.bmp", FileMode.Open, FileAccess.Read);
            BinaryReader r = new BinaryReader(fs);

            for (int i = 0; i < 10; i++)
                dummy = r.ReadByte();

            offset = r.ReadByte();
            offset += r.ReadByte() * 256;
            offset += r.ReadByte() * 256 * 256;
            offset += r.ReadByte() * 256 * 256 * 256;

            for (int i = 0; i < 4; i++)
                dummy = r.ReadByte();

            Size.Width = r.ReadByte();
            Size.Width += r.ReadByte() * 256;
            Size.Width += r.ReadByte() * 256 * 256;
            Size.Width += r.ReadByte() * 256 * 256 * 256;

            Size.Height = r.ReadByte();
            Size.Height += r.ReadByte() * 256;
            Size.Height += r.ReadByte() * 256 * 256;
            Size.Height += r.ReadByte() * 256 * 256 * 256;

            for (int i = 0; i < (offset - 26); i++)
                dummy = r.ReadByte();

            HeightData = new int[Size.Width, Size.Height];
            for (int y = 0; y < Size.Height; y++)
                for (int x = 0; x < Size.Width; x++)
                {
                    int height = (int)(r.ReadByte());
                    height += (int)(r.ReadByte());
                    height += (int)(r.ReadByte());
                    height /= 8;
                    HeightData[x, Size.Height - 1 - y] = height;
                }
        }

        private void VertexDeclaration(Device Device)
        {
            Vertices = new CustomVertex.PositionNormalColored[Size.Width * Size.Height];

            for (int x = 0; x < Size.Width; x++)
                for (int y = 0; y < Size.Height; y++)
                {
                    Vertices[x + y * Size.Width].Position = new Vector3(x * 64, y * 64, HeightData[x, y] == 0 ? -5000 : HeightData[x, y] * 8);
                    /*Vertices[x + y * Size.Width].Tu = x % 2 == 0 ? 0 : 1f;
                      Vertices[x + y * Size.Width].Tv = y % 2 == 0 ? 0 : 1f;*/
                    Vertices[x + y * Size.Width].Color = Color.FromArgb(HeightData[x, y] * HeightData[x, y] * HeightData[x, y] + 20000).ToArgb();
                }
        }

        private void IndicesDeclaration(Device Device)
        {
            Indices = new short[(Size.Width - 1) * (Size.Height - 1) * 6];

            for (int x = 0; x < Size.Width - 1; x++)
                for (int y = 0; y < Size.Height - 1; y++)
                {
                    Indices[(x + y * (Size.Width - 1)) * 6] = (short)(x + y * Size.Width);
                    Indices[(x + y * (Size.Width - 1)) * 6 + 1] = (short)((x + 1) + y * Size.Width);
                    Indices[(x + y * (Size.Width - 1)) * 6 + 2] = (short)((x + 1) + (y + 1) * Size.Width);

                    Indices[(x + y * (Size.Width - 1)) * 6 + 3] = (short)(x + (y + 1) * Size.Width);
                    Indices[(x + y * (Size.Width - 1)) * 6 + 4] = (short)(x + y * Size.Width);
                    Indices[(x + y * (Size.Width - 1)) * 6 + 5] = (short)((x + 1) + (y + 1) * Size.Width);
                }

        }

      /*private void VertexDeclaration(Device Device)
        {
            Vertices = new CustomVertex.PositionNormalColored[Size.Width * Size.Height * 4];

            for (int x = 0; x < Size.Width; x++)
                for (int y = 0; y < Size.Height; y++)
                {
                    int Height = HeightData[x, y] == 0 ? -5000 : HeightData[x, y] * 8;
                    int HeightColor = Color.FromArgb(HeightData[x, y] * HeightData[x, y] * HeightData[x, y] + 20000).ToArgb();

                    Vertices[(x + y * Size.Width) * 4].Position = new Vector3(x * 64, y * 64, Height);
                    Vertices[(x + y * Size.Width) * 4].Color = HeightColor;
                    Vertices[(x + y * Size.Width) * 4 + 1].Position = new Vector3(x * 64 + 32, y * 64, Height);
                    Vertices[(x + y * Size.Width) * 4 + 1].Color = HeightColor;
                    Vertices[(x + y * Size.Width) * 4 + 2].Position = new Vector3(x * 64 + 32, y * 64 + 32, Height);
                    Vertices[(x + y * Size.Width) * 4 + 2].Color = HeightColor;
                    Vertices[(x + y * Size.Width) * 4 + 3].Position = new Vector3(x * 64, y * 64 + 32, Height);
                    Vertices[(x + y * Size.Width) * 4 + 3].Color = HeightColor;

                    /*Vertices[x + y * Size.Width].Color = HeightData[x,y]==0? Color.FromArgb(0, 255-HeightData[x, y], 255-HeightData[x, y],255-HeightData[x,y]).ToArgb()
                          :Color.FromArgb(255,255,255,255).ToArgb();
                      Vertices[x + y * Size.Width].Color = Color.FromArgb(HeightData[x,y]* HeightData[x,y] *HeightData[x, y] + 20000).ToArgb();*/

                    /*Vertices[x + y * Size.Width].Tu = x % 2 == 0 ? 0 : 1f;
                      Vertices[x + y * Size.Width].Tv = y % 2 == 0 ? 0 : 1f;  
                }
        }

        private void IndicesDeclaration(Device Device)
        {
            Indices = new short[(Size.Width - 1) * (Size.Height - 1) * 3 * 2 * 4];

            for (int x = 0; x < Size.Width - 1; x++)
                for (int y = 0; y < Size.Height - 1; y++)
                {
                    Indices[(x + y * (Size.Width - 1)) * 24 + 0] = (short)((x + y * Size.Width) * 4);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 1] = (short)((x + y * Size.Width) * 4 + 1);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 2] = (short)((x + y * Size.Width) * 4 + 2);

                    Indices[(x + y * (Size.Width - 1)) * 24 + 3] = (short)((x + y * Size.Width) * 4 + 3);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 4] = (short)((x + y * Size.Width) * 4);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 5] = (short)((x + y * Size.Width) * 4 + 2);

                    Indices[(x + y * (Size.Width - 1)) * 24 + 6] = (short)((x + y * Size.Width) * 4 + 1);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 7] = (short)((x + 1 + y * Size.Width) * 4);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 8] = (short)((x + 1 + y * Size.Width) * 4 + 3);

                    Indices[(x + y * (Size.Width - 1)) * 24 + 9] = (short)((x + y * Size.Width) * 4 + 2);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 10] = (short)((x + y * Size.Width) * 4 + 1);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 11] = (short)((x + 1 + y * Size.Width) * 4 + 3);

                    Indices[(x + y * (Size.Width - 1)) * 24 + 12] = (short)((x + y * Size.Width) * 4 + 3);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 13] = (short)((x + y * Size.Width) * 4 + 2);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 14] = (short)((x + (y + 1) * Size.Width) * 4 + 1);

                    Indices[(x + y * (Size.Width - 1)) * 24 + 15] = (short)((x + (y+1) * Size.Width) * 4);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 16] = (short)((x + y * Size.Width) * 4+3);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 17] = (short)((x + (y+1) * Size.Width) * 4+1);

                    Indices[(x + y * (Size.Width - 1)) * 24 + 18] = (short)((x + y * Size.Width) * 4+2);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 19] = (short)((x+1 + y * Size.Width) * 4+3);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 20] = (short)((x+1 + (y+1) * Size.Width) * 4);

                    Indices[(x + y * (Size.Width - 1)) * 24 + 21] = (short)((x + (y+1) * Size.Width) * 4+1);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 22] = (short)((x + y * Size.Width) * 4+2);
                    Indices[(x + y * (Size.Width - 1)) * 24 + 23] = (short)((x+1 + (y+1) * Size.Width) * 4);

                }
        }*/

        private void GenerateNormals()
        {
            for (int Current = 0; Current < Vertices.Length; Current++)
                Vertices[Current].Normal = new Vector3(0, 0, 0);

            for (int Current = 0; Current < Indices.Length / 3; Current++)
            {
                Vector3 firstvec = Vertices[Indices[Current * 3 + 1]].Position - Vertices[Indices[Current * 3]].Position;
                Vector3 secondvec = Vertices[Indices[Current * 3]].Position - Vertices[Indices[Current * 3 + 2]].Position;
                Vector3 normal = Vector3.Cross(firstvec, secondvec);
                normal.Normalize();
                Vertices[Indices[Current * 3]].Normal += normal;
                Vertices[Indices[Current * 3 + 1]].Normal += normal;
                Vertices[Indices[Current * 3 + 2]].Normal += normal;
            }

            for (int Current = 0; Current < Vertices.Length; Current++)
                Vertices[Current].Normal.Normalize();
        }

        public float GetHeight(float worldX, float worldY)
        {
            int X = (int)(worldX / 64);
            int Y = (int)(worldY / 64);
            float offsetX = worldX - X*64;
            float offsetY = worldY - Y*64;

            if (offsetY < offsetX)
            {
                int[] Z = new int[3] { HeightData[X, Y], HeightData[X + 1, Y], HeightData[X + 1, Y + 1] };
                float vX = (float)(Z[1] - Z[0]) / 64;
                float vY = (float)(Z[2] - Z[1]) / 64;
                return (Z[0] + vX * offsetX + vY * offsetY) * 8;
            }
            else
            {
                int[] Z = new int[3] { HeightData[X, Y + 1], HeightData[X, Y], HeightData[X + 1, Y + 1] };
                float vX = (float)(Z[2] - Z[0]) / 64;
                float vY = (float)(Z[0] - Z[1]) / 64;
                return (Z[1] + vX * offsetX + vY * offsetY) * 8;
            }
        }

        private string CreateString(string Original, int Length)
        {
            string Return = Original;
            for (int Current = Original.Length; Current < Length; Current++)
                Return += ' ';
            return Return;
        }

        public void Render(Device Device)
        {
            Device.VertexFormat = CustomVertex.PositionNormalColored.Format;
            Device.Transform.World = Matrix.Translation(0, 0, 0);

           // Device.RenderState.Lighting = true;

          //  Device.Material = Material;
          //  Device.SetTexture(0, Tile);
            Device.SetTexture(0, Tile);
            Device.DrawIndexedUserPrimitives(PrimitiveType.TriangleList, 0, Vertices.Length, Indices.Length / 3, Indices, true, Vertices);

            Device.RenderState.Lighting = false;
        }
    }
}
