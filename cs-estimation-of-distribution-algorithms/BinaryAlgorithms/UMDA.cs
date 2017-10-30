using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDA;

namespace EDA.BinaryAlgorithms
{
    /// <summary>
    /// UMDA: Univariate Marginal Distribution Algorithm
    /// </summary>
    public class UMDA : BaseBinaryEDA
    {
        protected int mSelectionSize;
        protected int mPopSize;
        protected int mDimensionCount;

        public delegate int[] CreateSolutionMethod(object constraints);
        protected CreateSolutionMethod mSolutionGenerator;
        
        public UMDA(int pop_size, int dimension_count, int selection_size, CreateSolutionMethod solution_generator)
        {
            mPopSize = pop_size;
            mSelectionSize = selection_size;
            mDimensionCount = dimension_count;

            mSolutionGenerator = solution_generator;
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

            while (!should_terminate(improvement, iteration))
            {
                BinarySolution[] selected_solutions = BinaryTournamentSelection(pop, mSelectionSize); //order by ascending cost

                EstimateDistribution(selected_solutions, distribution_probabilities);


                for (int i = 0; i < mPopSize; ++i)
                {
                    int[] x_pi = Sample(distribution_probabilities);

                    double fx_pi = evaluate(x_pi, constraints);

                    pop[i] = new BinarySolution(x_pi, fx_pi);
                }

                pop = pop.OrderBy(s => s.Cost).ToArray();

                if(best_solution.TryUpdateSolution(pop[0].Values, pop[0].Cost, out improvement))
                {
                    OnSolutionUpdated(best_solution, iteration);
                }

                OnStepped(best_solution, iteration);
                iteration++;
            }

            return best_solution;
        }
    }
}
