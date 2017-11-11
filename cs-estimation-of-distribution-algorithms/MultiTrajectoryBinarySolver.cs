using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDA.ProblemModels;

namespace EDA
{
    public abstract class MultiTrajectoryBinarySolver : BinarySolver
    {
        public int MaxIterations { get; set; } = 2000;

        public abstract BinarySolution Minimize(CostEvaluationMethod evaluate, TerminationEvaluationMethod should_terminate, object constraints = null);

        public BinarySolution Minimize(CostEvaluationMethod evaluate, object constraints = null)
        {
            return Minimize(evaluate,
                (improvement, iterations) => {
                    return iterations >= MaxIterations;
                }, constraints);
        }
    }
}
