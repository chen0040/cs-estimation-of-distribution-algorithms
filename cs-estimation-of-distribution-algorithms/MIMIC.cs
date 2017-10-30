using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDA;

namespace EDA.BinaryAlgorithms
{
    public class MIMIC : BaseBinaryEDA
    {
        protected int mEliteCount;
        protected int mPopSize;
        protected int mDimensionCount;

        public delegate int[] CreateSolutionMethod(object constraints);
        protected CreateSolutionMethod mSolutionGenerator;

        public MIMIC(int pop_size, int dimension_count, int elite_count, CreateSolutionMethod generator)
        {
            mPopSize = pop_size;
            mEliteCount = elite_count;
            mDimensionCount = dimension_count;
            mSolutionGenerator = generator;

            if (mSolutionGenerator == null)
            {
                throw new NullReferenceException();
            }
        }

        public override BinarySolution Minimize(CostEvaluationMethod evaluate, TerminationEvaluationMethod should_terminate, object constraints = null)
        {
            double? improvement = null;
            int iteration = 0;

            BinarySolution[] pop = new BinarySolution[mPopSize];

            double min_fx_0 = double.MaxValue;
            int[] best_x_0 = null;
            for (int i = 0; i < mPopSize; ++i)
            {
                int[] x_0 = mSolutionGenerator(constraints);
                double fx_0 = evaluate(x_0, constraints);
                BinarySolution solution_0 = new BinarySolution(x_0, fx_0);
                pop[i] = solution_0;

                if (fx_0 < min_fx_0)
                {
                    min_fx_0 = fx_0;
                    best_x_0 = x_0;
                }
            }

            BinarySolution best_solution = new BinarySolution(best_x_0, min_fx_0);
            
            double[] distribution_probabilities = new double[mDimensionCount];

            EstimateDistribution(pop, distribution_probabilities);

            while (!should_terminate(improvement, iteration))
            {
                for (int i = 0; i < mPopSize; ++i)
                {
                    int[] x_pi = Sample(distribution_probabilities);

                    double fx_pi = evaluate(x_pi, constraints);

                    pop[i] = new BinarySolution(x_pi, fx_pi);
                }

                pop = pop.OrderBy(x => x.Cost).ToArray(); //order by ascending cost

                if(best_solution.TryUpdateSolution(pop[0].Values, pop[0].Cost, out improvement))
                {
                    OnSolutionUpdated(best_solution, iteration);
                }

                BinarySolution[] survived_solutions = new BinarySolution[mEliteCount];
                for (int i = 0; i < mEliteCount; ++i)
                {
                    survived_solutions[i] = pop[i];
                }

                EstimateDistribution(survived_solutions, distribution_probabilities);
              
                OnStepped(best_solution, iteration);
                iteration++;
            }

            return best_solution;
        }
    }
}
