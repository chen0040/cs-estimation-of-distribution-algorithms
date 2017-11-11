using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDA.ProblemModels;

namespace EDA.ContinuousAlgorithms
{
    public class CrossEntropyMethod : BaseContinuousEDA
    {
        protected int mSelectedSampleCount=5;
        protected int mSampleCount=50;
        protected int mDimensionCount;
        protected double mMinVariance = 10;
        protected double mLearnRate = 0.7;

        public delegate double[] CreateSolutionMethod(object constraints);
        public CreateSolutionMethod mSolutionGenerator;

        public CrossEntropyMethod(int pop_size, int selection_size, CostFunction f)
        {
            mSampleCount = pop_size;
            mSelectedSampleCount = selection_size;
            mDimensionCount = f.DimensionCount;

            mSolutionGenerator = (index) => f.CreateRandomSolution();
        }

        public CrossEntropyMethod(int pop_size, int dimension_count, int selection_size, CreateSolutionMethod solution_generator)
        {
            mSampleCount = pop_size;
            mSelectedSampleCount = selection_size;
            mDimensionCount = dimension_count;

            mSolutionGenerator = solution_generator;
            if (mSolutionGenerator == null)
            {
                throw new NullReferenceException();
            }
        }

        public double MinVariance
        {
            get { return mMinVariance; }
            set { mMinVariance = value; }
        }

        public double LearnRate
        {
            get { return mLearnRate; }
            set { mLearnRate = value; }
        }

        protected double MaxVariance(Gaussian[] distribution_functions)
        {
            return distribution_functions.Max(d => d.Variance);
        }

        public override ContinuousSolution Minimize(CostEvaluationMethod evaluate, GradientEvaluationMethod calc_gradient, TerminationEvaluationMethod should_terminate, object constraints = null)
        {
            double? improvement = null;
            int iteration = 0;

            ContinuousSolution[] samples = new ContinuousSolution[mSampleCount];
            ContinuousSolution[] selected_samples = new ContinuousSolution[mSelectedSampleCount];

            double min_fx_0 = double.MaxValue;
            double[] best_x_0 = null;
            for (int i = 0; i < mSampleCount; ++i)
            {
                double[] x_0 = mSolutionGenerator(constraints);
                double fx_0 = evaluate(x_0, mLowerBounds, mUpperBounds, constraints);
                ContinuousSolution solution_0 = new ContinuousSolution(x_0, fx_0);
                samples[i] = solution_0;

                if (fx_0 < min_fx_0)
                {
                    min_fx_0 = fx_0;
                    best_x_0 = x_0;
                }
            }

            ContinuousSolution best_solution = new ContinuousSolution(best_x_0, min_fx_0);

            Gaussian[] distribution_functions = new Gaussian[mDimensionCount];
            Gaussian[] selected_distribution_functions = new Gaussian[mDimensionCount];

            for (int i = 0; i < mDimensionCount; ++i)
            {
                distribution_functions[i] = new Gaussian();
                selected_distribution_functions[i] = new Gaussian();
            }

            EstimateDistribution(samples, distribution_functions);

            //while (MaxVariance(distribution_functions) <= mMinVariance)
            while(!should_terminate(improvement, iteration))
            {
                for (int i = 0; i < mSampleCount; ++i)
                {
                    double[] x = Sample(distribution_functions);

                    double fx = evaluate(x, mLowerBounds, mUpperBounds, constraints);

                    ContinuousSolution solution = new ContinuousSolution(x, fx);
                    samples[i] = solution;
                }

                samples = samples.OrderBy(s => s.Cost).ToArray();

                if(best_solution.TryUpdateSolution(samples[0].Values, samples[0].Cost, out improvement))
                {
                    OnSolutionUpdated(best_solution, iteration);
                }

                for (int i = 0; i < mSelectedSampleCount; ++i)
                {
                    selected_samples[i] = samples[i];
                }

                EstimateDistribution(selected_samples, selected_distribution_functions);

                for (int i = 0; i < mDimensionCount; ++i)
                {
                    distribution_functions[i].Mean = mLearnRate * distribution_functions[i].Mean +  (1 - mLearnRate) * selected_distribution_functions[i].Mean;
                    distribution_functions[i].StdDev = mLearnRate * distribution_functions[i].StdDev + (1 - mLearnRate) * selected_distribution_functions[i].StdDev;
                }

                OnStepped(best_solution, iteration);
                iteration++;
            }

            return best_solution;
        }
    }
}
