using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Core.Model;
using MathNet.Numerics.LinearAlgebra;

namespace FEMur.Core.Analyze
{
    public class Result
    {
        //解析結果を格納するクラス
        public FEMModel model { get; set; }
        public Matrix<double>? d { get; set; }
        public Matrix<double>?  stress { get; set; }

        public Result(FEMModel model)
        {
            this.model = model;
            this.stress = Matrix<double>.Build.Dense(model.nodes.Count, 3);
        }
    }
}
