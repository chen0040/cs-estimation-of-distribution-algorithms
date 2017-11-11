using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDA.Benchmarks;
using EDA.BinaryAlgorithms;
using EDA.ProblemModels;

namespace EDA
{
    public class UT_PBIL_Binary
    {
        public static void RunMain()
        {
            int popSize = 8000;
            int dimension = 50;
            int eliteCount = 50;
            PBIL s = new PBIL(popSize, dimension, eliteCount);
            s.MaxIterations = 100;

            s.SolutionUpdated += (best_solution, step) =>
            {
                Console.WriteLine("Step {0}: Fitness = {1}", step, best_solution.Cost);
            };

            s.Minimize((solution, constraints) =>
            {
                // solution is binary-encoded
                double cost = 0;
                // minimize the number of 1 bits in the solution
                for(int i=0; i < solution.Length; ++i)
                {
                    cost += solution[i]; 
                }
                return cost;
            });
        }
    }
}
