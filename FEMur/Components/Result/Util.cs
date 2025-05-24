using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;
using Rhino;
using FEMur.Core.FEMur2D.Model;

namespace FEMur.Components.Result
{
    internal class Util
    {
        public static List<Color> GetColors(List<double> value)
        {
            List<Color> colors = new List<Color>();
            double minVal = value.Min();
            double maxVal = value.Max();
            foreach (double val in value)
            {
                double ratio = (val - minVal) / (maxVal - minVal);
                colors.Add(GetJetColor(ratio));
            }
            return colors;
        }

        public static Color GetJetColor(double value)
        {
            value = Math.Max(0.0, Math.Min(1.0, value)); // 0-1にクランプ
            double r = Math.Max(0, Math.Min(1, 1.5 - Math.Abs(4 * value - 3)));
            double g = Math.Max(0, Math.Min(1, 1.5 - Math.Abs(4 * value - 2)));
            double b = Math.Max(0, Math.Min(1, 1.5 - Math.Abs(4 * value - 1)));
            Color color = Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
            return color;
        }

        public static Mesh deformationMesh(FEMModel model, double d_ratio)
        {
            List<Point3d> deformation = new List<Point3d>();
            for (int i = 0; i < model.nodes.Count; i++)
            {
                deformation.Add(new Point3d(model.nodes[i].x + model.result.d[i * 2, 0] * d_ratio, model.nodes[i].y + model.result.d[i * 2 + 1, 0] * d_ratio, model.nodes[i].z));
            }
            //deformatinoをMeshに変換
            Mesh meshes = new Mesh();
            foreach(Point3d p in deformation)
            {
                meshes.Vertices.Add(p);
            }
            for (int i = 0; i < model.elements.Count; i++)
            {
                meshes.Faces.AddFace(model.elements[i].nodes_id[0], model.elements[i].nodes_id[1], model.elements[i].nodes_id[2], model.elements[i].nodes_id[3]);
            }
            return meshes;
        }

        public static List<Curve> getContourLines(Mesh mesh,List<double> stressValues)
        {
            List<Curve> curves = new List<Curve>();
            double minVal = stressValues.Min();
            double maxVal = stressValues.Max();

            int COLORLEVEL = 10;

            double[] isoLevels = new double[COLORLEVEL];

            for (int i = 0; i < COLORLEVEL; i++)
            {
                isoLevels[i] = minVal + (maxVal - minVal) / (double)(COLORLEVEL -1) * i;
            }

            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                // フェイスの頂点インデックス取得
                int[] vertexIndices = { mesh.Faces[i].A, mesh.Faces[i].B, mesh.Faces[i].C };
                if (mesh.Faces[i].IsQuad)
                    vertexIndices = new int[] { mesh.Faces[i].A, mesh.Faces[i].B, mesh.Faces[i].C, mesh.Faces[i].D };


                // 各頂点の位置と応力値
                Point3d[] points = vertexIndices.Select(idx => (Point3d)mesh.Vertices[idx]).ToArray();
                double[] values = vertexIndices.Select(idx => stressValues[idx]).ToArray();
                // エッジごとに等高線を作成
                for (int j = 1; j < isoLevels.Length - 1; j++)
                {
                    double isoValue = isoLevels[j];
                    List<Point3d> contourPoints = new List<Point3d>();
                    for (int k = 0; k < points.Length; k++) //エッジごとの処理
                    {
                        int next = (k + 1) % points.Length;
                        // 線分の端点の応力値
                        double v1 = values[k], v2 = values[next];
                        // もし両方の端点が同じ側にあるなら無視
                        if ((v1 > isoValue && v2 > isoValue) || (v1 < isoValue && v2 < isoValue)) continue;
                        // 線分上の補間点を求める
                        if (Math.Abs(v1 - v2) > 1e-6)
                        {
                            double t = (isoValue - v1) / (v2 - v1);
                            Point3d interpPoint = points[k] + t * (points[next] - points[k]);
                            contourPoints.Add(interpPoint);
                        }
                    }
                    if (contourPoints.Count == 2)
                    {
                        curves.Add(new Line(contourPoints[0], contourPoints[1]).ToNurbsCurve());
                    }
                }
            }
            return curves;
        }
    }
}
