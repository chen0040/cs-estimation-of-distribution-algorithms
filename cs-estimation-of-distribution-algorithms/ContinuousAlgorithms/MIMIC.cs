using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDA.ProblemModels;

namespace EDA.ContinuousAlgorithms
{
    public class MIMIC : BaseContinuousEDA
    {
        protected int mEliteCount;
        protected int mPopSize;
        protected int mDimensionCount;

        public delegate double[] CreateSolutionMethod(object constraints);
        protected CreateSolutionMethod mSolutionGenerator;
        
        public MIMIC(int pop_size, int dimension_count, int elite_count, CreateSolutionMethod solution_generator = null)
        {
            mPopSize = pop_size;
            mEliteCount = elite_count;
            mDimensionCount = dimension_count;

            mSolutionGenerator = solution_generator;
        }

        public MIMIC(int popSize, CostFunction f)
        {
            mPopSize = popSize;
            mEliteCount = Math.Max(2, (int)(popSize * 0.05));
            mDimensionCount = f.DimensionCount;
            mLowerBounds = f.LowerBounds;
            mUpperBounds = f.UpperBounds;

            mSolutionGenerator = (constraints) =>
            {
                return f.CreateRandomSolution();
            };
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

            EstimateDistribution(population, distribution_functions);

            while (!should_terminate(improvement, iteration))
            {
                for (int i = 0; i < mPopSize; ++i)
                {
                    double[] x_pi = Sample(distribution_functions);

                    double fx_pi = evaluate(x_pi, mLowerBounds, mUpperBounds, constraints);

                    population[i] = new ContinuousSolution(x_pi, fx_pi);
                }

                population = population.OrderBy(x => x.Cost).ToArray(); //order by ascending cost

                if(best_solution.TryUpdateSolution(population[0].Values, population[0].Cost, out improvement))
                {
                    OnSolutionUpdated(best_solution, iteration);
                }

                ContinuousSolution[] survived_solutions = new ContinuousSolution[mEliteCount];
                for (int i = 0; i < mEliteCount; ++i)
                {
                    survived_solutions[i] = population[i];
                }

                EstimateDistribution(survived_solutions, distribution_functions);
              
                OnStepped(best_solution, iteration);
                iteration++;
            }

            return best_solution;
        }
    }
}
