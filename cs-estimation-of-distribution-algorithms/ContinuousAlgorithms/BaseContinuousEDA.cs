using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDA.ProblemModels;

namespace EDA.ContinuousAlgorithms
{
    public abstract class BaseContinuousEDA : ContinuousSolver
    {
        public BaseContinuousEDA()
        {
     
        }

        public virtual ContinuousSolution Minimize(CostFunction f, int max_iterations)
        {
            return Minimize((x, lower_bounds, upper_bounds, constraints) =>
            {
                return f.Evaluate(x);
            },
                (x, gradX, lower_bounds, upper_bounds, constraints) =>
                {
                    f.CalcGradient(x, gradX);
                },
                (improvement, iterations) =>
                {
                    return iterations > max_iterations;
                });
        }

        public abstract ContinuousSolution Minimize(CostEvaluationMethod evaluate, GradientEvaluationMethod calc_gradient, TerminationEvaluationMethod should_terminate, object constraints = null);

        protected ContinuousSolution[] BinaryTournamentSelection(ContinuousSolution[] population, int selection_size)
        {
            ContinuousSolution[] selected_solutions = new ContinuousSolution[selection_size];
            int population_size = population.Length;
            for (int i = 0; i < selection_size; ++i)
            {
                int index1 = RandomEngine.NextInt(population_size);
                int index2 = index1;
                do
                {
                    index2 = RandomEngine.NextInt(population_size);
                } while (index1 == index2);

                ContinuousSolution solution1 = population[index1];
                ContinuousSolution solution2 = population[index2];

                if (solution1.IsBetterThan(solution2))
                {
                    selected_solutions[i] = solution1;
                }
                else
                {
                    selected_solutions[i] = solution2;
                }
            }

            return selected_solutions;
        }

        protected virtual void EstimateDistribution(ContinuousSolution[] solutions, Gaussian[] distributions)
        {
            int dimension_count = distributions.Length;
            int solution_count = solutions.Length;
            for (int i = 0; i < dimension_count; ++i)
            {
                double sum = 0;
                for (int j = 0; j < solution_count; ++j)
                {
                    ContinuousSolution solution = solutions[j];
                    double val = solution.Values[i];
                    sum += val;
                }
                double mu = sum / solution_count;
                sum = 0;
                for (int j = 0; j < solution_count; ++j)
                {
                    ContinuousSolution solution = solutions[j];
                    double val = solution.Values[i];
                    double dVal = val - mu;
                    sum += dVal * dVal;
                }
                double sigma2 = sum / solution_count;
                double sigma = System.Math.Sqrt(sigma2);
                distributions[i].Mean = mu;
                distributions[i].StdDev = sigma;
            }
        }

        public double[] Sample(Gaussian[] distributions)
        {
            int dimension_count = distributions.Length;
            double[] sample = new double[dimension_count];
            for (int i = 0; i < dimension_count; ++i)
            {
                sample[i] = distributions[i].Next();
            }
            return sample;
        }
    }
}
