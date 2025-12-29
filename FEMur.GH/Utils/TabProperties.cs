using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel;

namespace FEMurGH.Utils
{
    public class TabProperties : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
             var server = Grasshopper.Instances.ComponentServer;
            server.AddCategoryShortName("FEMur", "FEM");
            server.AddCategorySymbolName("FEMur", 'F');
            server.AddCategoryIcon("FEMur", Properties.Resources.FEMur);

            return GH_LoadingInstruction.Proceed;
        }

    }
}
