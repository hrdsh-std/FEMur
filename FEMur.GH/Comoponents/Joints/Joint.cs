using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using FEMur.Elements;
using FEMur.Joints;

namespace FEMurGH.Comoponents.Joints
{
    /// <summary>
    /// 要素の材端接合条件（Joint）を作成するコンポーネント
    /// </summary>
    public class JointComponent : GH_Component
    {
        public JointComponent()
          : base("Joint(FEMur)", "Joint",
              "Create FEMur Joints for element end connections (Pin, Rigid, Semi-Rigid)",
              "FEMur", "3.Element")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements", "E", "FEMur Elements (LineElement)", GH_ParamAccess.list);
            pManager.AddIntegerParameter("JointType", "T", "Joint type: 0=Rigid-Rigid, 1=Pin-Pin, 2=Pin-Rigid, 3=Rigid-Pin, 4=Semi-Rigid", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Start RX", "SRX", "Start end rotational spring stiffness around X [N·m/rad] (for Semi-Rigid)", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Start RY", "SRY", "Start end rotational spring stiffness around Y [N·m/rad] (for Semi-Rigid)", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Start RZ", "SRZ", "Start end rotational spring stiffness around Z [N·m/rad] (for Semi-Rigid)", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("End RX", "ERX", "End rotational spring stiffness around X [N·m/rad] (for Semi-Rigid)", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("End RY", "ERY", "End rotational spring stiffness around Y [N·m/rad] (for Semi-Rigid)", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("End RZ", "ERZ", "End rotational spring stiffness around Z [N·m/rad] (for Semi-Rigid)", GH_ParamAccess.item, 0.0);

            // Semi-Rigid用のパラメータはオプション
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
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

            int jointType = 0;
            DA.GetData(1, ref jointType);

            double startRx = 0, startRy = 0, startRz = 0;
            double endRx = 0, endRy = 0, endRz = 0;

            DA.GetData(2, ref startRx);
            DA.GetData(3, ref startRy);
            DA.GetData(4, ref startRz);
            DA.GetData(5, ref endRx);
            DA.GetData(6, ref endRy);
            DA.GetData(7, ref endRz);

            var joints = new List<Joint>(elements.Count);

            foreach (var element in elements)
            {
                if (element == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Null element skipped");
                    continue;
                }

                Joint joint;
                switch (jointType)
                {
                    case 0: // Rigid-Rigid
                        joint = Joint.CreateRigid(element);
                        break;
                    case 1: // Pin-Pin
                        joint = Joint.CreatePin(element);
                        break;
                    case 2: // Pin-Rigid
                        joint = Joint.CreatePinRigid(element);
                        break;
                    case 3: // Rigid-Pin
                        joint = Joint.CreateRigidPin(element);
                        break;
                    case 4: // Semi-Rigid
                        joint = Joint.CreateSemiRigid(element, startRx, startRy, startRz, endRx, endRy, endRz);
                        break;
                    default:
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Invalid joint type: {jointType}. Use 0-4.");
                        return;
                }

                joints.Add(joint);
            }

            DA.SetDataList(0, joints);
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("A7F3E8D2-4B6C-4E9A-B1D5-8C2F7A9E3B4D");
    }
}