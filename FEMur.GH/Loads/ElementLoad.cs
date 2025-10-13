using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Elements;
using FEMur.Geometry;

namespace FEMurGH.Loads
{
    public class ElementLoad : GH_Component
    {
        public ElementLoad()
          : base("ElementLoad(FEMur)", "ElementLoad",
              "Create FEMur ElementLoads from Elements with distributed load vectors",
              "FEMur", "Model")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements", "E", "FEMur Elements (List<FEMur.Elements.ElementBase>)", GH_ParamAccess.list);
            pManager.AddVectorParameter("Force", "F", "Distributed force vector [N/mm] (qx, qy, qz in local coordinates)", GH_ParamAccess.item, Vector3d.Zero);
            pManager.AddVectorParameter("Moment", "M", "Distributed moment vector [N·mm/mm] (mx, my, mz in local coordinates)", GH_ParamAccess.item, Vector3d.Zero);
            pManager.AddBooleanParameter("isLocal", "L", "If true, vectors are defined in local coordinates (typically true for element loads)", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ElementLoads", "EL", "FEMur ElementLoads", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var elements = new List<ElementBase>();
            if (!DA.GetDataList(0, elements) || elements.Count == 0)
                return;

            Vector3d ghF = Vector3d.Zero;
            Vector3d ghM = Vector3d.Zero;
            bool isLocal = true;

            if (!DA.GetData(1, ref ghF)) return;
            if (!DA.GetData(2, ref ghM)) return;
            if (!DA.GetData(3, ref isLocal)) return;

            var qLocal = new Vector3(ghF.X, ghF.Y, ghF.Z);
            var mLocal = new Vector3(ghM.X, ghM.Y, ghM.Z);

            var loads = new List<FEMur.Loads.ElementLoad>(elements.Count);
            for (int i = 0; i < elements.Count; i++)
            {
                var elem = elements[i];
                if (elem == null) continue;
                loads.Add(new FEMur.Loads.ElementLoad(elem, qLocal, mLocal, isLocal));
            }

            DA.SetDataList(0, loads);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("94962CAF-456C-434B-8C7E-40E1851C297F");
    }
}