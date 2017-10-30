using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDA;

namespace EDA.BinaryAlgorithms
{
    /// <summary>
    /// Compact Genetic Algorithm
    /// </summary>
    public class CGA : BaseBinaryEDA
    {
        protected double m_n;
        protected int mDimensionCount;

        public delegate int[] CreateSolutionMethod(object constraints);
        protected CreateSolutionMethod mSolutionGenerator;
        
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

        public double VectorUpdateParam
        {
            get { return m_n; }
            set { m_n = value; }
        }

        public override BinarySolution Minimize(CostEvaluationMethod evaluate, TerminationEvaluationMethod should_terminate, object constraints = null)
        {
            double? improvement = null;
            int iteration = 0;

            int[] x_0 = mSolutionGenerator(constraints);
            double fx_0 = evaluate(x_0, constraints);

            BinarySolution best_solution = new BinarySolution(x_0, fx_0);
            
            double[] distribution_probabilities = new double[mDimensionCount];

            for (int i = 0; i < mDimensionCount; ++i)
            {
                distribution_probabilities[i] = 0.5;
            }

            while (!should_terminate(improvement, iteration))
            {    
                int[] x1 = Sample(distribution_probabilities);
                double fx1 = evaluate(x1, constraints);

                int[] x2 = Sample(distribution_probabilities);
                double fx2 = evaluate(x2, constraints);

                int[] x_winner = fx1 < fx2 ? x1 : x2;
                double fx_winner = fx1 < fx2 ? fx1 : fx2;

                if(best_solution.TryUpdateSolution(x_winner, fx_winner, out improvement))
                {
                    OnSolutionUpdated(best_solution, iteration);
                }

                for (int i = 0; i < mDimensionCount; ++i)
                {
                    if (x1[i] != x2[i])
                    {
                        if (x1[i] == 1)
                        {
                            distribution_probabilities[i] += 1 / m_n;
                        }
                        else
                        {
                            distribution_probabilities[i] -= 1 / m_n;
                        }
                    }
                }

                OnStepped(best_solution, iteration);
                iteration++;
            }

            return best_solution;
        }
    }
}
