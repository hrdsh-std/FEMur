using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace FEMurGH
{
    public class FEMur_GHInfo : GH_AssemblyInfo
    {
        public override string Name => "FEMur";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("b766d672-4707-42f1-911f-f18380cfac8d");

        //Return a string identifying you or your company.
        public override string AuthorName => "Hrdsh";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";

        //Return a string representing the version.  This returns the same version as the assembly.
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
    }
}