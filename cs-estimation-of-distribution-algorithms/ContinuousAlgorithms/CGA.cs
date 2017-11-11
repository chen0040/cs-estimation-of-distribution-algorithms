using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDA.Helpers;
using EDA.ProblemModels;

namespace EDA.ContinuousAlgorithms
{
    /// <summary>
    /// Compact Genetic Algorithm
    /// </summary>
    public class CGA : BaseContinuousEDA
    {
        protected int m_n;
        protected int mDimensionCount;

        public delegate double[] CreateSolutionMethod(object constraints);
        protected CreateSolutionMethod mSolutionGenerator;

        public CGA(int n, CostFunction f)
        {
            m_n = n;
            mDimensionCount = f.DimensionCount;

            mSolutionGenerator = (constraints) =>
            {
                return f.CreateRandomSolution();
            };
        }

        public CGA(int n, int dimension_count, int selection_size, CreateSolutionMethod solution_generator)
        {
            m_n = n;
            mDimensionCount = dimension_count;

            mSolutionGenerator = solution_generator;
            if (mSolutionGenerator == null)
            {
                throw new NullReferenceException();
            }
        }

        public int VectorUpdateParam
        {
            get { return m_n; }
            set { m_n = value; }
        }

        public override ContinuousSolution Minimize(CostEvaluationMethod evaluate, GradientEvaluationMethod calc_gradient, TerminationEvaluationMethod should_terminate, object constraints = null)
        {
            double? improvement = null;
            int iteration = 0;

            double[] x_0 = mSolutionGenerator(constraints);
            double fx_0 = evaluate(x_0, mLowerBounds, mUpperBounds, constraints);

            ContinuousSolution best_solution = new ContinuousSolution(x_0, fx_0);

            Gaussian[] distribution_probabilities = new Gaussian[mDimensionCount];

            MinPQ<ContinuousSolution> recent_winners = new MinPQ<ContinuousSolution>((a, b) =>
            {
                return b.Cost.CompareTo(a.Cost);
            }); // DelMin will delete the solution with highest cost

            for (int i = 0; i < mDimensionCount; ++i)
            {
                distribution_probabilities[i] = new Gaussian(0, 1);
            }

            while (!should_terminate(improvement, iteration))
            {    
                double[] x1 = Sample(distribution_probabilities);
                double fx1 = evaluate(x1, mLowerBounds, mUpperBounds, constraints);

                double[] x2 = Sample(distribution_probabilities);
                double fx2 = evaluate(x2, mLowerBounds, mUpperBounds, constraints);

                double[] x_winner = fx1 < fx2 ? x1 : x2;
                double fx_winner = fx1 < fx2 ? fx1 : fx2;

                if(best_solution.TryUpdateSolution(x_winner, fx_winner, out improvement))
                {
                    OnSolutionUpdated(best_solution, iteration);
                }

                ContinuousSolution winner = new ContinuousSolution(x_winner, fx_winner);

                recent_winners.Add(winner);

                if(recent_winners.Count > m_n)
                {
                    recent_winners.DelMin();
                }

                EstimateDistribution(recent_winners.ToArray(), distribution_probabilities);

                OnStepped(best_solution, iteration);
                iteration++;
            }

            return best_solution;
        }
    }
}
