using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace BeyondInfinity_Editor
{
    public class Group
    {
        public string Name;
        public short[] Indicies;
        public byte MaterialIndex;
    }

    public class Object
    {
        public Vector3 Location;
        public float Rotation;

        private CustomVertex.PositionNormalTextured[] Vertices;
        private byte[] Vertices_BoneID;
        private short[] Indices;

        private Group[] Groups;
        private Material[] Materials;
        private Texture[] Textures;

        public Object(Device Device,string Path, string Name, Vector3 location)
        {
            Location = location;

            //try
            {
                FileStream Stream = new FileStream(Path+@"\"+Name, FileMode.Open, FileAccess.Read);
                BinaryReader File = new BinaryReader(Stream);

                //Header
                string Header = Encoding.ASCII.GetString(File.ReadBytes(10));
                if (Header != "MS3D000000")
                {
                    MessageBox.Show("Not A Valid Milkshape3D Model File! (" + ")", "Error while loading Model!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (File.ReadInt32() != 4)
                {
                    MessageBox.Show("Not Supported Model Version! (Version 4 is supported only)", "Error while loading Model!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //Vertices
                short Vertices_Number = File.ReadInt16();
                Vertices = new CustomVertex.PositionNormalTextured[Vertices_Number];
                Vertices_BoneID = new byte[Vertices_Number];
                //MessageBox.Show("Loading " + Vertices_Number + " Vertices..", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                for (short Current = 0; Current < Vertices_Number; Current++)
                {
                    Vertices[Current] = new CustomVertex.PositionNormalTextured();
                    File.ReadByte(); //Flags
                    Vertices[Current].X = File.ReadSingle();
                    Vertices[Current].Y = File.ReadSingle();
                    Vertices[Current].Z = File.ReadSingle();
                    Vertices_BoneID[Current] = File.ReadByte();//BoneID
                    File.ReadByte();//ReferenceCount
                }

                //Triangles
                short Triangles_Number = File.ReadInt16();
                Indices = new short[Triangles_Number * 3];
                //MessageBox.Show("Loading " + Triangles_Number + " Triangles..", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                for (short Current = 0; Current < Triangles_Number; Current++)
                {
                    File.ReadInt16(); //Flags

                    for (short Indicie = 0; Indicie < 3; Indicie++)
                        Indices[Current * 3 + Indicie] = File.ReadInt16();

                    for (short Normal = 0; Normal < 3; Normal++)
                    {
                        Vertices[Indices[Current * 3 + Normal]].Nx = File.ReadSingle();
                        Vertices[Indices[Current * 3 + Normal]].Ny = File.ReadSingle();
                        Vertices[Indices[Current * 3 + Normal]].Nz = File.ReadSingle();
                    }

                    Vertices[Indices[Current * 3 + 0]].Tu = File.ReadSingle();
                    Vertices[Indices[Current * 3 + 1]].Tu = File.ReadSingle();
                    Vertices[Indices[Current * 3 + 2]].Tu = File.ReadSingle();
                    Vertices[Indices[Current * 3 + 0]].Tv = File.ReadSingle();
                    Vertices[Indices[Current * 3 + 1]].Tv = File.ReadSingle();
                    Vertices[Indices[Current * 3 + 2]].Tv = File.ReadSingle();

                    File.ReadByte(); //SmoothingGroup
                    File.ReadByte(); //GroupIndex
                }

                //Edges
                //MessageBox.Show("Setting up edges..", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //Groups
                short Groups_Number = File.ReadInt16();
                Groups = new Group[Groups_Number];
                //MessageBox.Show("Loading " + Groups_Number + " Groups..", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                for (short Current = 0; Current < Groups_Number; Current++)
                {
                    Groups[Current] = new Group();
                    File.ReadByte(); //Flags
                    Groups[Current].Name = Encoding.ASCII.GetString(File.ReadBytes(32)).Split(new char[] { '\0' }, 2)[0];

                    short Indicies_Number = File.ReadInt16();
                    Groups[Current].Indicies = new short[Indicies_Number * 3];
                    for (short Indicie = 0; Indicie < Indicies_Number; Indicie++)
                    {
                        short Index = File.ReadInt16(); //Groups[Current].TriangleIndices[Indicie] =
                        Groups[Current].Indicies[Indicie * 3 + 0] = Indices[Index * 3 + 0];
                        Groups[Current].Indicies[Indicie * 3 + 1] = Indices[Index * 3 + 1];
                        Groups[Current].Indicies[Indicie * 3 + 2] = Indices[Index * 3 + 2];
                    }

                    Groups[Current].MaterialIndex = File.ReadByte();
                }

                //Materials
                short Materials_Number = File.ReadInt16();
                Materials = new Material[Materials_Number];
                Textures = new Texture[Materials_Number];
                //MessageBox.Show("Loading " + Materials_Number + " Materials..", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                for (short Current = 0; Current < Materials_Number; Current++)
                {
                    string name = Encoding.ASCII.GetString(File.ReadBytes(32)).Split(new char[] { '\0' }, 2)[0];

                    Materials[Current].Ambient = Color.FromArgb((int)File.ReadSingle(), (int)File.ReadSingle(), (int)File.ReadSingle(), (int)File.ReadSingle());
                    Materials[Current].Diffuse = Color.FromArgb((int)File.ReadSingle(), (int)File.ReadSingle(), (int)File.ReadSingle(), (int)File.ReadSingle());
                    Materials[Current].Specular = Color.FromArgb((int)File.ReadSingle(), (int)File.ReadSingle(), (int)File.ReadSingle(), (int)File.ReadSingle());
                    Materials[Current].Emissive = Color.FromArgb((int)File.ReadSingle(), (int)File.ReadSingle(), (int)File.ReadSingle(), (int)File.ReadSingle());

                    float Shininess = File.ReadSingle();
                    float Transparency = File.ReadSingle();

                    byte Mode = File.ReadByte();
                    string Texture = Encoding.ASCII.GetString(File.ReadBytes(128)).Split(new char[] { '\0' }, 2)[0];
                    Textures[Current] = TextureLoader.FromFile(Device, Path + @"\" + Texture);
                    string AlphaMap = Encoding.ASCII.GetString(File.ReadBytes(128)).Split(new char[] { '\0' }, 2)[0];
                }

                Stream.Close();
                File.Close();
            }
            /*catch (Exception E)
              {
                  MessageBox.Show(E.Message, "Error while loading Model!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                  return;
              }*/

            //Generating Normals
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

        public void Render(Device Device)
        {
            Device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
            Device.Transform.World = Matrix.RotationX((float)Math.PI / 2) * Matrix.RotationZ(-Rotation / 100) * Matrix.Translation(Location.X, Location.Y, Location.Z);

            for (int Current = 0; Current < Groups.Length; Current++)
            {
                Device.Material = Materials[Groups[Current].MaterialIndex];
                Device.SetTexture(0, Textures[Groups[Current].MaterialIndex]);
                Device.DrawIndexedUserPrimitives(PrimitiveType.TriangleList, 0, Vertices.Length, Groups[Current].Indicies.Length / 3, Groups[Current].Indicies, true, Vertices);
            }
        }
    }
}
