using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Core.Common
{
    public interface INode
    {
        int ID { get; }
        double X { get; }
        double Y { get; }
        double Z { get; }　//２次元の場合は0.0とする。

        string ToString();
    }
}
