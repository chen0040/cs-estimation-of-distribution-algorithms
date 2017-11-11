using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDA.Benchmarks;
using EDA.ContinuousAlgorithms;
using EDA.ProblemModels;

namespace EDA
{
    public class UT_CGA_Continuous
    {
        public static void Run(CostFunction f, int max_iterations=200000)
        {
            int n = 1000;
            CGA s = new CGA(n, f);


            s.SolutionUpdated += (best_solution, step) =>
            {
                Console.WriteLine("Step {0}: Fitness = {1}", step, best_solution.Cost);
            };

            s.Minimize(f, max_iterations);
        }

        public static void Run_Sphere()
        {
            CostFunction_Sphere f = new CostFunction_Sphere();
            Run(f);
        }

        public static void RunRosenbrockSaddle(int max_iterations=2000)
        {
            CostFunction_RosenbrockSaddle f = new CostFunction_RosenbrockSaddle();
            Run(f, max_iterations);
        }
    }
}
