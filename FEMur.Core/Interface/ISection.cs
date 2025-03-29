using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEMur.Core.Interface
{
    public interface ISection
    {
        double Thickness { get; }
        string ToString();
    }
}
