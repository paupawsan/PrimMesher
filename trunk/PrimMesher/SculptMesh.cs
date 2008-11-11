using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace PrimMesher
{
    public class SculptMesh
    {
        public List<Coord> coords;
        public List<Face> faces;

        public List<ViewerFace> viewerFaces;
        private List<Coord> normals;
        private List<UVCoord> uvs;

        public enum SculptType { sphere = 1, torus = 2, plane = 3 };
        private const float pixScale = 0.00390625f;

        public SculptMesh(Bitmap bitmap, SculptType sculptType, int lod, bool viewerMode)
        {
            coords = new List<Coord>();
            faces = new List<Face>();
            normals = new List<Coord>();
            uvs = new List<UVCoord>();

            viewerFaces = new List<ViewerFace>();

            int width = bitmap.Width;
            int height = bitmap.Height;

            float widthUnit = 1.0f / width;
            float heightUnit = 1.0f / height;

            for (int imageY = 0; imageY < height; imageY++)
            {
                int rowOffset = imageY * width;

                for (int imageX = 0; imageX < width; imageX++)
                {
                    Color color = bitmap.GetPixel(imageX, imageY);

                    float x = color.R * pixScale - 0.5f;
                    float y = color.G * pixScale - 0.5f;
                    float z = color.B * pixScale - 0.5f;

                     /*
                     * u1,v1   u2,v1
                     *   p1-----p2
                     *   | \ f2 |
                     *   |   \  |
                     *   | f1  \|
                     *   p3-----p4
                     * u1,v2   u2,v2
                     */
                    int p4 = rowOffset + imageX;
                    int p3 = p4 - 1;
                    int p2 = p4 - width;
                    int p1 = p3 - width;

                    Coord c = new Coord(x, y, z);
                    this.coords.Add(c);
                    if (viewerMode)
                    {
                        this.normals.Add(new Coord());
                        this.uvs.Add(new UVCoord(widthUnit * imageX, heightUnit * imageY));
                    }

                    if (imageY > 0 && imageX > 0)
                    {
                        Face f1 = new Face(p1, p3, p4);
                        Face f2 = new Face(p1, p4, p2);

                        this.faces.Add(f1);
                        this.faces.Add(f2);

                        //if (viewerMode)
                        //{
                        //    float u2 = widthUnit * imageX;
                        //    float u1 = u2 - widthUnit;
                        //    float v2 = heightUnit * imageY;
                        //    float v1 = v2 - heightUnit;

                        //    ViewerFace vf1 = new ViewerFace(0);
                        //    vf1.v1 = coords[f1.v1];
                        //    vf1.v2 = coords[f1.v2];
                        //    vf1.v3 = coords[f1.v3];
                        //    vf1.CalcSurfaceNormal();  //todo - vertex normals
                        //    vf1.uv1.U = u1;
                        //    vf1.uv1.V = v1;
                        //    vf1.uv2.U = u1;
                        //    vf1.uv2.V = v2;
                        //    vf1.uv3.U = u2;
                        //    vf1.uv3.V = v2;

                        //    ViewerFace vf2 = new ViewerFace(0);
                        //    vf2.v1 = coords[f2.v1];
                        //    vf2.v2 = coords[f2.v2];
                        //    vf2.v3 = coords[f2.v3];
                        //    vf2.CalcSurfaceNormal();
                        //    vf2.uv1.U = u1;
                        //    vf2.uv1.V = v1;
                        //    vf2.uv2.U = u2;
                        //    vf2.uv2.V = v2;
                        //    vf2.uv3.U = u2;
                        //    vf2.uv3.V = v1;

                        //    this.viewerFaces.Add(vf1);
                        //    this.viewerFaces.Add(vf2);

                        //}
                    }
                }
            }

            if (viewerMode)
            {  // compute vertex normals by summing all the surface normals of all the triangles sharing
                // each vertex and then normalizing
                int numFaces = this.faces.Count;
                for (int i = 0; i < numFaces; i++)
                {
                    Face face = this.faces[i];
                    Coord surfaceNormal = face.SurfaceNormal(this.coords);
                    this.normals[face.v1] += surfaceNormal;
                    this.normals[face.v2] += surfaceNormal;
                    this.normals[face.v3] += surfaceNormal;
                }

                int numCoords = this.coords.Count;
                for (int i = 0; i < numCoords; i++)
                    this.coords[i].Normalize();

                foreach (Face face in this.faces)
                {
                    ViewerFace vf = new ViewerFace(0);
                    vf.v1 = this.coords[face.v1];
                    vf.v2 = this.coords[face.v2];
                    vf.v3 = this.coords[face.v3];

                    vf.n1 = this.normals[face.v1];
                    vf.n2 = this.normals[face.v2];
                    vf.n3 = this.normals[face.v3];

                    vf.uv1 = this.uvs[face.v1];
                    vf.uv2 = this.uvs[face.v2];
                    vf.uv3 = this.uvs[face.v3];

                    this.viewerFaces.Add(vf);
                }
            }
        }

        public void AddRot(Quat q)
        {
            Console.WriteLine("AddRot(" + q.ToString() + ")");
            int i;
            int numVerts = this.coords.Count;

            for (i = 0; i < numVerts; i++)
                this.coords[i] *= q;

            if (this.viewerFaces != null)
            {
                int numViewerFaces = this.viewerFaces.Count;

                for (i = 0; i < numViewerFaces; i++)
                {
                    ViewerFace v = this.viewerFaces[i];
                    v.v1 *= q;
                    v.v2 *= q;
                    v.v3 *= q;

                    v.n1 *= q;
                    v.n2 *= q;
                    v.n3 *= q;

                    this.viewerFaces[i] = v;
                }
            }
        }

        public void Scale(float x, float y, float z)
        {
            int i;
            int numVerts = this.coords.Count;
            //Coord vert;

            Coord m = new Coord(x, y, z);
            for (i = 0; i < numVerts; i++)
                this.coords[i] *= m;

            if (this.viewerFaces != null)
            {
                int numViewerFaces = this.viewerFaces.Count;
                for (i = 0; i < numViewerFaces; i++)
                {
                    ViewerFace v = this.viewerFaces[i];
                    v.v1 *= m;
                    v.v2 *= m;
                    v.v3 *= m;
                    this.viewerFaces[i] = v;
                }
            }
        }

        public void DumpRaw(String path, String name, String title)
        {
            if (path == null)
                return;
            String fileName = name + "_" + title + ".raw";
            String completePath = Path.Combine(path, fileName);
            StreamWriter sw = new StreamWriter(completePath);

            for (int i = 0; i < this.faces.Count; i++)
            {
                string s = this.coords[this.faces[i].v1].ToString();
                s += " " + this.coords[this.faces[i].v2].ToString();
                s += " " + this.coords[this.faces[i].v3].ToString();

                sw.WriteLine(s);
            }

            sw.Close();
        }
    }
}
