using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Core.Common;

namespace FEMur.Core.Interface
{
    public interface IFemModel
    {
        public List<INode> Nodes { get; }
        public List<IElement> Elements { get; }
        public List<ISupport> Supports { get; }
        public List<ILoad> Loads { get; }

        string ToString();

    }
}
