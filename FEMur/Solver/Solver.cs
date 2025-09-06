using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEMur.Models;
using FEMur.Results;


namespace FEMur.Solver
{
    public abstract class Solver
    {
        public abstract Result Solve(Model model);
    }
}
