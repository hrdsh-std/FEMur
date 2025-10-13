using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using FEMur.Results;
using FEMur.Models;
using MathNet.Numerics.LinearAlgebra;

namespace FEMurGH.Results
{
    public class DeconstructResult : GH_Component
    {
        public DeconstructResult()
          : base("DeconstructResult(FEMur)", "DeconstructResult",
              "Deconstruct FEMur AnalyzedModel's analysis result into displacements and stresses",
              "FEMur", "Results")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("AnalyzedModel", "AM", "FEMur Model with computed results", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("NodalDisplacements", "D", "Nodal displacement vector (raw MathNet Vector)", GH_ParamAccess.item);
            pManager.AddGenericParameter("ElementStresses", "S", "List of element stresses (List<ElementStress>)", GH_ParamAccess.list);
            pManager.AddNumberParameter("DisplacementValues", "DV", "Displacement values as flat list", GH_ParamAccess.list);
            pManager.AddTextParameter("StressInfo", "SI", "Stress information as text", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Model model = null;

            if (!DA.GetData(0, ref model))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "AnalyzedModel is required");
                return;
            }

            if (!model.IsSolved || model.Result == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Model has not been solved yet. Run LinearStaticSolver first.");
                return;
            }

            try
            {
                var result = model.Result;

                // 変位ベクトル（MathNet Vector<double>）
                var displacements = result.NodalDisplacements;
                
                // 変位を数値リストに変換
                var dispValues = new List<double>();
                if (displacements != null)
                {
                    for (int i = 0; i < displacements.Count; i++)
                    {
                        dispValues.Add(displacements[i]);
                    }
                }

                // 応力情報をテキストに変換
                var stressInfo = new List<string>();
                if (result.ElementStresses != null)
                {
                    foreach (var stress in result.ElementStresses)
                    {
                        stressInfo.Add(stress.ToString());
                    }
                }

                // 出力
                DA.SetData(0, displacements);
                DA.SetDataList(1, result.ElementStresses);
                DA.SetDataList(2, dispValues);
                DA.SetDataList(3, stressInfo);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error deconstructing result: {ex.Message}");
            }
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("5474AA2D-A504-46CA-A1EF-52EAF3D03E48");
    }
}