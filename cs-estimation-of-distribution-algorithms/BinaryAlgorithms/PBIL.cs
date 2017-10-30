// ***********************************************************************
// Assembly         : SimuKit.Solvers
// Author           : chen0469
// Created          : 07-29-2013
//
// Last Modified By : chen0469
// Last Modified On : 05-06-2013
// ***********************************************************************
// <copyright file="PBILOptimizer.cs" company="Meme Analytics">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// The PBIL namespace.
/// </summary>
namespace EDA.BinaryAlgorithms.PBIL
{
    using EDA;
    public class PBIL : BaseBinaryEDA
    {
        protected int mPopSize;
        protected int mDimensionCount;
        
        protected double mLearnRate;
        protected double mNegLearnRate;
        protected double mMutProb;
        protected double mMutShift;

        public delegate int[] CreateSolutionMethod(object constraints);
        protected CreateSolutionMethod mSolutionGenerator;

        public PBIL(int pop_size, int dimension_count, int elite_count, CreateSolutionMethod solution_generator, double learnRate = 0.1, double negLearnRate = 0.075, double mutProb = 0.02, double mutShift = 0.05)
        {
            mPopSize = pop_size;
            mDimensionCount = dimension_count;

            mLearnRate = learnRate;
            mNegLearnRate = negLearnRate;
            mMutProb = mutProb;
            mMutShift = mutShift;

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

            double[] distribution_functions = new double[mDimensionCount];

            EstimateDistribution(pop, distribution_functions);

            while (!should_terminate(improvement, iteration))
            {
                for (int i = 0; i < mPopSize; ++i)
                {
                    int[] x_pi = Sample(distribution_functions);

                    double fx_pi = evaluate(x_pi, constraints);

                    pop[i] = new BinarySolution(x_pi, fx_pi);
                }

                pop = pop.OrderBy(x => x.Cost).ToArray(); //order by ascending cost

                if (best_solution.TryUpdateSolution(pop[0].Values, pop[0].Cost, out improvement))
                {
                    OnSolutionUpdated(best_solution, iteration);
                }

                BinarySolution best_current_solution = pop[0];
                BinarySolution worst_current_solution = pop[mPopSize - 1];

                if (best_solution.TryUpdateSolution(best_current_solution.Values, best_current_solution.Cost, out improvement))
                {
                    OnSolutionUpdated(best_solution, iteration);
                }

                // Update the probability vector with max and min cost solutions
                for (int i = 0; i < mDimensionCount; ++i)
                {
                    if (best_current_solution.Values[i] == worst_current_solution.Values[i])
                    {
                        double oldMean = distribution_functions[i];
                        distribution_functions[i] = oldMean * (1 - mLearnRate) + best_current_solution.Values[i] * mLearnRate;
                    }
                    else
                    {
                        double learnRate2 = mLearnRate + mNegLearnRate;
                        double oldMean = distribution_functions[i];

                        distribution_functions[i] = oldMean * (1 - learnRate2) + best_current_solution.Values[i] * learnRate2;
                    }
                }

                // Mutation
                for (int i = 0; i < mDimensionCount; i++)
                {
                    if (RandomEngine.NextDouble() < mMutProb)
                    {
                        double oldMean = distribution_functions[i];

                        distribution_functions[i] = oldMean * (1 - mMutShift) +
                                (RandomEngine.NextBoolean() ? 1 : 0) * mMutShift;
                    }
                }

                OnStepped(best_solution, iteration);
                iteration++;
            }

            return best_solution;
            
        }
    }
}
