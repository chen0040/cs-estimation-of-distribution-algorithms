using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDA.ContinuousAlgorithms
{
    /// <summary>
    /// UMDA: Univariate Marginal Distribution Algorithm
    /// </summary>
    public class UMDA : BaseContinuousEDA
    {
        protected int mSelectionSize;
        protected int mPopSize;
        protected int mDimensionCount;
        public delegate double[] CreateSolutionMethod(object constraints);
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

        public override ContinuousSolution Minimize(CostEvaluationMethod evaluate, GradientEvaluationMethod calc_gradient, TerminationEvaluationMethod should_terminate, object constraints = null)
        {
            double? improvement = null;
            int iteration = 0;

            ContinuousSolution[] population = new ContinuousSolution[mPopSize];

            double min_fx_0 = double.MaxValue;
            double[] best_x_0 = null;
            for (int i = 0; i < mPopSize; ++i)
            {
                double[] x_0 = mSolutionGenerator(constraints);
                double fx_0 = evaluate(x_0, mLowerBounds, mUpperBounds, constraints);
                ContinuousSolution solution_0 = new ContinuousSolution(x_0, fx_0);
                population[i] = solution_0;

                if (fx_0 < min_fx_0)
                {
                    min_fx_0 = fx_0;
                    best_x_0 = x_0;
                }
            }

            ContinuousSolution best_solution = new ContinuousSolution(best_x_0, min_fx_0);

            Gaussian[] distribution_functions = new Gaussian[mDimensionCount];

            for (int i = 0; i < mDimensionCount; ++i)
            {
                distribution_functions[i] = new Gaussian();
            }

            while (!should_terminate(improvement, iteration))
            {
                ContinuousSolution[] selected_solutions = BinaryTournamentSelection(population, mSelectionSize); //order by ascending cost

                EstimateDistribution(selected_solutions, distribution_functions);


                for (int i = 0; i < mPopSize; ++i)
                {
                    double[] x_pi = Sample(distribution_functions);

                    double fx_pi = evaluate(x_pi, mLowerBounds, mUpperBounds, constraints);

                    population[i] = new ContinuousSolution(x_pi, fx_pi);
                }

                population = population.OrderBy(s => s.Cost).ToArray();

                if(best_solution.TryUpdateSolution(population[0].Values, population[0].Cost, out improvement))
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
