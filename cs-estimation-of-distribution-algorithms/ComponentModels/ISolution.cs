using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDA.ProblemModels;

namespace EDA.ComponentModels
{
    public interface ISolution
    {
        ISolution Clone();
    }
}
