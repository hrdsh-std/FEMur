using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace FEMur.Results
{
    public class Result
    {
        public Vector<double> NodalDisplacements { get; set; }

        public Result() { }

    }
}
