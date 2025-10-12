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
        /// <summary>
        /// 節点変位ベクトル（全自由度）
        /// </summary>
        public Vector<double> NodalDisplacements { get; set; }

        /// <summary>
        /// 要素ごとの断面力（応力）
        /// </summary>
        public List<ElementStress> ElementStresses { get; set; }

        public Result()
        {
            ElementStresses = new List<ElementStress>();
        }
    }
}
