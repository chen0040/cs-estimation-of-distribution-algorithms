// ***********************************************************************
// Assembly         : SimuKit.Solvers
// Author           : chen0469
// Created          : 07-29-2013
//
// Last Modified By : chen0469
// Last Modified On : 05-06-2013
// ***********************************************************************
// <copyright file="PBILOptimizer.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDA.ProblemModels;

/// <summary>
/// The PBIL namespace.
/// </summary>
namespace EDA.ContinuousAlgorithms
{
    public class PBIL : BaseContinuousEDA
    {
        protected int mEliteCount;
        protected int mPopSize;
        protected int mDimensionCount;
        
        protected double mLearnRate;
        protected double mNegLearnRate;
        protected double mMutProb;
        protected double mMutShift;

        public delegate double[] CreateSolutionMethod(object constraints);
        protected CreateSolutionMethod mSolutionGenerator;

        public PBIL(int popSize, CostFunction f, double learnRate = 0.1, double negLearnRate = 0.075, double mutProb = 0.02, double mutShift = 0.05)
        {
            mPopSize = popSize;
            mEliteCount = Math.Max(2, (int)(popSize * 0.05));
            mDimensionCount = f.DimensionCount;


            mLearnRate = learnRate;
            mNegLearnRate = negLearnRate;
            mMutProb = mutProb;
            mMutShift = mutShift;

            mLowerBounds = f.LowerBounds;
            mUpperBounds = f.UpperBounds;

            mSolutionGenerator = (constraints) =>
            {
                return f.CreateRandomSolution();
            };
        }

        public PBIL(int pop_size, int dimension_count, int elite_count, CreateSolutionMethod solution_generator, double learnRate = 0.1, double negLearnRate = 0.075, double mutProb = 0.02, double mutShift = 0.05)
        {
            mPopSize = pop_size;
            mEliteCount = elite_count;
            mDimensionCount = dimension_count;
        

            mLearnRate = learnRate;
            mNegLearnRate = negLearnRate;
            mMutProb = mutProb;
            mMutShift = mutShift;

            mSolutionGenerator=solution_generator;
            if(mSolutionGenerator == null)
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

                if (best_solution.TryUpdateSolution(population[0].Values, population[0].Cost, out improvement))
                {
                    OnSolutionUpdated(best_solution, iteration);
                }

                ContinuousSolution[] survived_solutions=new ContinuousSolution[mEliteCount];
                for(int i=0; i  < mEliteCount; ++i)
                {
                    survived_solutions[i]=population[i];
                }

                EstimateDistribution(survived_solutions, distribution_functions);

                ContinuousSolution best_current_solution = population[0];
                ContinuousSolution worst_current_solution = population[mPopSize - 1];

                if (best_solution.TryUpdateSolution(best_current_solution.Values, best_current_solution.Cost, out improvement))
                {
                    OnSolutionUpdated(best_solution, iteration);
                }

                // Update the probability vector with max and min cost solutions
                for (int i = 0; i < mDimensionCount; ++i)
                {
                    if (best_current_solution.Values[i] == worst_current_solution.Values[i])
                    {
                        double oldMean = distribution_functions[i].Mean;
                        distribution_functions[i].Mean = oldMean * (1 - mLearnRate) + best_current_solution.Values[i] * mLearnRate;
                    }
                    else
                    {
                        double learnRate2 = mLearnRate + mNegLearnRate;
                        double oldMean = distribution_functions[i].Mean;

                        distribution_functions[i].Mean = oldMean * (1 - learnRate2) + best_current_solution.Values[i] * learnRate2;
                    }
                }

                // Mutation
                for (int i = 0; i < mDimensionCount; i++)
                {
                    if (RandomEngine.NextDouble() < mMutProb)
                    {
                        double oldMean = distribution_functions[i].Mean;

                        distribution_functions[i].Mean = oldMean * (1 - mMutShift) +
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
