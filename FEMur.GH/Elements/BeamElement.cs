using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

// コア型のエイリアス（名前の衝突を避ける）
using FEMNode = FEMur.Nodes.Node;
using FEMMat = FEMur.Materials.Material;
using FEMCS = FEMur.CrossSections.CrossSection_Beam;
using FEMBeamElement = FEMur.Elements.BeamElement;

namespace FEMurGH.Elements
{
    /// <summary>
    /// 入力: List<Node> i端Node, List<Node> j端Node, Material, CrossSection_Beam, BetaAngle
    /// 出力: List<FEMur.Elements.BeamElement>
    /// </summary>
    public class BeamElement : GH_Component
    {
        public BeamElement()
          : base("BeamElement(FEMur)", "BeamElement",
              "Create FEMur BeamElements from paired i/j Nodes with material, cross section, and beta angle.",
              "FEMur", "Model")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            // Generic 入力として FEMur の型を受け取る（Goo 未実装のため）
            pManager.AddGenericParameter("iNodes", "Ni", "Start-end (i) FEMur Nodes (List<Node>)", GH_ParamAccess.list);
            pManager.AddGenericParameter("jNodes", "Nj", "End-end (j) FEMur Nodes (List<Node>)", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "Mat", "FEMur Material", GH_ParamAccess.item);
            pManager.AddGenericParameter("CrossSection", "CS", "FEMur CrossSection_Beam", GH_ParamAccess.item);
            pManager.AddNumberParameter("BetaAngle", "β", "Local coordinate system rotation angle (degrees, default=0)", GH_ParamAccess.item, 0.0);

            // BetaAngleはオプショナル
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements", "E", "FEMur BeamElements", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var iNodes = new List<FEMNode>();
            var jNodes = new List<FEMNode>();
            FEMMat material = null;
            FEMCS crossSection = null;
            double betaAngle = 0.0;

            if (!DA.GetDataList(0, iNodes)) return;
            if (!DA.GetDataList(1, jNodes)) return;
            if (!DA.GetData(2, ref material)) return;
            if (!DA.GetData(3, ref crossSection)) return;
            DA.GetData(4, ref betaAngle); // オプショナル

            if (material == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Material is null.");
                return;
            }
            if (crossSection == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "CrossSection_Beam is null.");
                return;
            }
            if (iNodes.Count != jNodes.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"iNodes.Count ({iNodes.Count}) must equal jNodes.Count ({jNodes.Count}).");
                return;
            }
            if (iNodes.Count == 0)
            {
                DA.SetDataList(0, new List<FEMBeamElement>());
                return; 
            }

            var elems = new List<FEMBeamElement>(iNodes.Count);
            for (int k = 0; k < iNodes.Count; k++)
            {
                var ni = iNodes[k];
                var nj = jNodes[k];
                if (ni == null || nj == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Node pair at index {k} contains null. Skipped.");
                    continue;
                }

                // 要素IDは0からの連番で採番（必要に応じて別入力で開始IDを設けても良い）
                var be = new FEMBeamElement(k, ni, nj, material, crossSection, betaAngle);
                elems.Add(be);
            }

            DA.SetDataList(0, elems);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("83675EC7-F3CA-405D-A7D6-84290BC6955B");
    }
}