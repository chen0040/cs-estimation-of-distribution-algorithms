using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDA;

namespace EDA.BinaryAlgorithms
{
    public abstract class BaseBinaryEDA : BinarySolver
    {
        public BaseBinaryEDA()
        {
     
        }

        public abstract BinarySolution Minimize(CostEvaluationMethod evaluate, TerminationEvaluationMethod should_terminate, object constraints = null);

        protected BinarySolution[] BinaryTournamentSelection(BinarySolution[] population, int selection_size)
        {
            BinarySolution[] selected_solutions = new BinarySolution[selection_size];
            int population_size = population.Length;
            for (int i = 0; i < selection_size; ++i)
            {
                int index1 = RandomEngine.NextInt(population_size);
                int index2 = index1;
                do
                {
                    index2 = RandomEngine.NextInt(population_size);
                } while (index1 == index2);

                BinarySolution solution1 = population[index1];
                BinarySolution solution2 = population[index2];

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

        protected virtual void EstimateDistribution(BinarySolution[] solutions, double[] distributions)
        {
            int dimension_count = distributions.Length;
            int solution_count = solutions.Length;
            for (int i = 0; i < dimension_count; ++i)
            {
                double sum = 0;
                for (int j = 0; j < solution_count; ++j)
                {
                    BinarySolution solution = solutions[j];
                    int val = solution.Values[i];
                    sum += val;
                }
                double mu = sum / solution_count;
                distributions[i] = mu;
            }
        }

        public int[] Sample(double[] distributions)
        {
            int dimension_count = distributions.Length;
            int[] sample = new int[dimension_count];
            for (int i = 0; i < dimension_count; ++i)
            {
                sample[i] = RandomEngine.NextBoolean(distributions[i]) ? 1 : 0;
            }
            return sample;
        }
    }
}
