using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using FEMur.Elements;
using FEMur.Joints;

namespace FEMurGH.Comoponents.Joints
{
    /// <summary>
    /// カスタム接合条件を作成するコンポーネント
    /// 各自由度ごとに固定/解放を設定可能
    /// </summary>
    public class CustomJointComponent : GH_Component
    {
        public CustomJointComponent()
          : base("CustomJoint(FEMur)", "CJoint",
              "Create FEMur Joints with custom DOF release settings",
              "FEMur", "3.Element")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements", "E", "FEMur Elements (LineElement)", GH_ParamAccess.list);
            pManager.AddTextParameter("Name", "N", "Joint name", GH_ParamAccess.item, "Custom");

            // Start端の設定（6自由度: DX, DY, DZ, RX, RY, RZ）
            pManager.AddBooleanParameter("Start DX Fixed", "SDX", "Fix Start end DX (true=fixed, false=free/spring)", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Start DY Fixed", "SDY", "Fix Start end DY", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Start DZ Fixed", "SDZ", "Fix Start end DZ", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Start RX Fixed", "SRX", "Fix Start end RX", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Start RY Fixed", "SRY", "Fix Start end RY", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Start RZ Fixed", "SRZ", "Fix Start end RZ", GH_ParamAccess.item, true);

            // End端の設定
            pManager.AddBooleanParameter("End DX Fixed", "EDX", "Fix End DX", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("End DY Fixed", "EDY", "Fix End DY", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("End DZ Fixed", "EDZ", "Fix End DZ", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("End RX Fixed", "ERX", "Fix End RX", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("End RY Fixed", "ERY", "Fix End RY", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("End RZ Fixed", "ERZ", "Fix End RZ", GH_ParamAccess.item, true);

            // ばね剛性（解放された自由度用）
            pManager.AddNumberParameter("Start Rot Stiffness", "SRS", "Start rotational spring stiffness [RX, RY, RZ] [N·m/rad]", GH_ParamAccess.list);
            pManager.AddNumberParameter("End Rot Stiffness", "ERS", "End rotational spring stiffness [RX, RY, RZ] [N·m/rad]", GH_ParamAccess.list);

            pManager[1].Optional = true;
            pManager[14].Optional = true;
            pManager[15].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Joints", "J", "FEMur Joints", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var elements = new List<LineElement>();
            if (!DA.GetDataList(0, elements) || elements.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No elements provided");
                return;
            }

            string name = "Custom";
            DA.GetData(1, ref name);

            // Start端のフラグ
            bool sDX = true, sDY = true, sDZ = true, sRX = true, sRY = true, sRZ = true;
            DA.GetData(2, ref sDX);
            DA.GetData(3, ref sDY);
            DA.GetData(4, ref sDZ);
            DA.GetData(5, ref sRX);
            DA.GetData(6, ref sRY);
            DA.GetData(7, ref sRZ);

            // End端のフラグ
            bool eDX = true, eDY = true, eDZ = true, eRX = true, eRY = true, eRZ = true;
            DA.GetData(8, ref eDX);
            DA.GetData(9, ref eDY);
            DA.GetData(10, ref eDZ);
            DA.GetData(11, ref eRX);
            DA.GetData(12, ref eRY);
            DA.GetData(13, ref eRZ);

            // ばね剛性
            var startRotStiffness = new List<double> { 0, 0, 0 };
            var endRotStiffness = new List<double> { 0, 0, 0 };
            DA.GetDataList(14, startRotStiffness);
            DA.GetDataList(15, endRotStiffness);

            // リストサイズを3に調整
            while (startRotStiffness.Count < 3) startRotStiffness.Add(0);
            while (endRotStiffness.Count < 3) endRotStiffness.Add(0);

            var joints = new List<Joint>(elements.Count);

            foreach (var element in elements)
            {
                if (element == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Null element skipped");
                    continue;
                }

                var joint = new Joint(element, name);

                // Start端の設定
                joint.IsFixed[0] = sDX;
                joint.IsFixed[1] = sDY;
                joint.IsFixed[2] = sDZ;
                joint.IsFixed[3] = sRX;
                joint.IsFixed[4] = sRY;
                joint.IsFixed[5] = sRZ;

                // End端の設定
                joint.IsFixed[6] = eDX;
                joint.IsFixed[7] = eDY;
                joint.IsFixed[8] = eDZ;
                joint.IsFixed[9] = eRX;
                joint.IsFixed[10] = eRY;
                joint.IsFixed[11] = eRZ;

                // ばね剛性の設定
                joint.SetStartRotationalStiffness(startRotStiffness[0], startRotStiffness[1], startRotStiffness[2]);
                joint.SetEndRotationalStiffness(endRotStiffness[0], endRotStiffness[1], endRotStiffness[2]);

                joints.Add(joint);
            }

            DA.SetDataList(0, joints);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("B8F4E9D3-5C7D-4F0B-C2E6-9D3F8B0E4C5E");
    }
}