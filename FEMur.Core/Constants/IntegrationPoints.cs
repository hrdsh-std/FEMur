using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Core.Constants
{
    public enum ElementType
    {
        Triangle3,  // 3節点三角形要素
        Quad4       // 4節点四角形要素
    }
    public enum IntegrationScheme
    {
        Gauss1,
        Gauss3,
        Gauss4,
        Gauss7
    }
    //積分点を表す構造体
    public struct GaussPoint
    {
        public double[] Ls { get; set; }
        public double Weight { get; set; }

        public GaussPoint(double[] Ls, double Weight)
        {
            this.Ls = Ls;
            this.Weight = Weight;
        }
    }

    //積分点を静的クラスで管理
    public static class GaussIntegration
    {

        private static double[] TriangleIP7Alpha = { 1.0 / 3.0, 0.05971587, 0.47014206, 0.79742669, 0.10128651 };
        //三角形の積分点。重心座標系での積分点と重み係数
        public static readonly Dictionary<IntegrationScheme, GaussPoint[]> Triangle3 = new()
        {
            { IntegrationScheme.Gauss1, new GaussPoint[] { new GaussPoint(new double[] { 1.0 / 3.0, 1.0 / 3.0 }, 1.0) } },
            { IntegrationScheme.Gauss3, new GaussPoint[] {
                new GaussPoint(new double[] { 1.0 / 2.0, 1.0 / 2.0 , 0.0 }, 1.0 / 3.0),
                new GaussPoint(new double[] { 0.0 , 1.0 / 2.0, 1.0 / 2.0 }, 1.0 / 3.0),
                new GaussPoint(new double[] { 1.0 / 2.0, 0.0 , 1.0 / 2.0 }, 1.0 / 3.0)//重心座標系の３点積分のLsはこれであってるの？
            } },
            //{ IntegrationScheme.Gauss4, new IntegrationPoint[] {
            //    new IntegrationPoint(new double[] { 1.0 / 3.0, 1.0 / 3.0 }, -27.0 / 96.0),
            //    new IntegrationPoint(new double[] { 0.6, 0.2 }, 25.0 / 96.0),
            //    new IntegrationPoint(new double[] { 0.2, 0.6 }, 25.0 / 96.0),
            //    new IntegrationPoint(new double[] { 0.2, 0.2 }, 25.0 / 96.0)
            //} },
            //{ IntegrationScheme.Gauss9, new IntegrationPoint[] {
            //    new IntegrationPoint(new double[] { 1.0 / 3.0, 1.0 / 3.0 }, 0.225),
            //    new IntegrationPoint(new double[] { 0.797426985353087, 0.101286507323456 }, 0.125939180544827),
            //    new IntegrationPoint(new double[] { 0.101286507323456, 0.797426985353087 }, 0.125939180544827),
            //    new IntegrationPoint(new double[] { 0.101286507323456, 0.101286507323456 }, 0.125939180544827),
            //    new IntegrationPoint(new double[] { 0.059715871789770, 0.470142064105115 }, 0.132
            { IntegrationScheme.Gauss7, new GaussPoint[] {
                new GaussPoint(new double[] { TriangleIP7Alpha[0], TriangleIP7Alpha[0], TriangleIP7Alpha[0] }, 0.225),
                new GaussPoint(new double[] { TriangleIP7Alpha[1], TriangleIP7Alpha[2], TriangleIP7Alpha[2] }, 0.13239415),
                new GaussPoint(new double[] { TriangleIP7Alpha[2], TriangleIP7Alpha[1], TriangleIP7Alpha[2] }, 0.13239415),//重心座標系の３点積分のLsはこれであってるの？
                new GaussPoint(new double[] { TriangleIP7Alpha[2], TriangleIP7Alpha[2], TriangleIP7Alpha[1] }, 0.13239415),
                new GaussPoint(new double[] { TriangleIP7Alpha[3], TriangleIP7Alpha[4], TriangleIP7Alpha[4] }, 0.12593918),
                new GaussPoint(new double[] { TriangleIP7Alpha[4], TriangleIP7Alpha[3], TriangleIP7Alpha[4] }, 0.12593918),
                new GaussPoint(new double[] { TriangleIP7Alpha[4], TriangleIP7Alpha[4], TriangleIP7Alpha[3] }, 0.12593918)
            } }
        };

        //四角形の積分点 は必要に応じてここに追加

        //積分点を取得するメソッド
        public static GaussPoint[] GetPoints(ElementType type, IntegrationScheme scheme)
        {
            return type switch
            {
                ElementType.Triangle3 => Triangle3.ContainsKey(scheme) ? Triangle3[scheme] : throw new ArgumentException("Invalid integration scheme"),
                //ElementType.Quad4 => Quad4.ContainsKey(scheme) ? Quad4[scheme] : throw new ArgumentException("Invalid integration scheme"),
                _ => throw new ArgumentException("Invalid element type")
            };
        }



    }
}
