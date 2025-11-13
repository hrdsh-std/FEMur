using System;
using System.Collections.Generic;

namespace FEMur.Geometry.Intermediate
{
    /// <summary>
    /// FEMur内部のジオメトリ型と中間表現型の相互変換ユーティリティ
    /// </summary>
    public static class GeometryConverter
    {
        /// <summary>
        /// Point3からGeometryNodeへ変換
        /// </summary>
        public static GeometryNode ToGeometryNode(Point3 point)
        {
            return new GeometryNode(point);
        }

        /// <summary>
        /// Point3のリストからGeometryNodeのリストへ変換
        /// </summary>
        public static List<GeometryNode> ToGeometryNodes(IEnumerable<Point3> points)
        {
            var nodes = new List<GeometryNode>();
            foreach (var point in points)
            {
                nodes.Add(ToGeometryNode(point));
            }
            return nodes;
        }

        /// <summary>
        /// 2つのPoint3からGeometryLineを生成
        /// </summary>
        public static GeometryLine ToGeometryLine(Point3 start, Point3 end)
        {
            return new GeometryLine(start, end);
        }

        /// <summary>
        /// Point3ペアのリストからGeometryLineのリストを生成
        /// </summary>
        public static List<GeometryLine> ToGeometryLines(IEnumerable<(Point3 start, Point3 end)> linePairs)
        {
            var lines = new List<GeometryLine>();
            foreach (var (start, end) in linePairs)
            {
                lines.Add(ToGeometryLine(start, end));
            }
            return lines;
        }

        /// <summary>
        /// GeometryNodeからPoint3へ変換
        /// </summary>
        public static Point3 ToPoint3(GeometryNode node)
        {
            return node.Position;
        }

        /// <summary>
        /// GeometryNodeのリストからPoint3のリストへ変換
        /// </summary>
        public static List<Point3> ToPoint3List(IEnumerable<GeometryNode> nodes)
        {
            var points = new List<Point3>();
            foreach (var node in nodes)
            {
                points.Add(ToPoint3(node));
            }
            return points;
        }
    }
}
