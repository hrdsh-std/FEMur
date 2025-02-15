using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEMur.Components.Test
{
    public class Test : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Test()
          : base("TEST", "Test",
              "Description",
              "FEMur", "Test")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            if(!DA.GetData(0, ref mesh)) return;
            // メッシュの重心
            Point3d centroid = mesh.GetBoundingBox(true).Center;

            // メッシュの法線の平均を計算
            Vector3d normal = Vector3d.Zero;
            mesh.Normals.ComputeNormals(); // 法線を計算
            for (int i = 0; i < mesh.Normals.Count; i++)
            {
                normal += mesh.Normals[i];
            }
            normal.Unitize();

            // 元のメッシュの参照平面を作成
            Plane sourcePlane = new Plane(centroid, normal);

            // XY平面への変換行列を作成
            Transform transform = Transform.PlaneToPlane(sourcePlane, Plane.WorldXY);

            // メッシュを変換
            Mesh transformedMesh = mesh.DuplicateMesh();
            transformedMesh.Transform(transform);

            // 出力
            DA.SetData(0, transformedMesh);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C9E13C84-5C15-40EB-A93B-16FBB097882A"); }
        }
    }
}