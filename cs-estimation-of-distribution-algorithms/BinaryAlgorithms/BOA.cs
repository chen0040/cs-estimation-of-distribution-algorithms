using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDA;

namespace EDA.BinaryAlgorithms
{
    /// <summary>
    /// BOA implements Bayesian Optimization Algorithm
    /// </summary>
    public class BOA : BaseBinaryEDA
    {
        protected int mPopSize = 50;
        protected int mSelectedSampleSize = 15;
        protected int mNumChildren = 25;
        protected int mDimension;

        public delegate int[] CreateSolutionMethod(object constraints);
        protected CreateSolutionMethod mSolutionGenerator;

        public int SelectedSampleSize
        {
            get { return mSelectedSampleSize; }
            set { mSelectedSampleSize = value; }
        }

        public int NumChildren
        {
            get { return mNumChildren; }
            set { mNumChildren = value; }
        }

        public int PopSize
        {
            get { return mPopSize; }
            set { mPopSize = value; }
        }

        public BOA(int pop_size, int dimension, CreateSolutionMethod solution_generator)
        {
            mPopSize = pop_size;
            mDimension = dimension;

            mSolutionGenerator = solution_generator;
            if (mSolutionGenerator == null)
            {
                throw new NullReferenceException();
            }
        }

        public int Dimension
        {
            get { return mDimension; }
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

            while (!should_terminate(improvement, iteration))
            {
                pop = pop.OrderBy(s => s.Cost).ToArray();

                if (best_solution.TryUpdateSolution(pop[0].Values, pop[0].Cost, out improvement))
                {
                    OnSolutionUpdated(best_solution, iteration);
                }

                BayesianGraph g = new BayesianGraph(mDimension);

                g.ConstructNetwork(pop);

                BinarySolution[] children = g.Sample(pop, mNumChildren);

                foreach(BinarySolution s in children)
                {
                    s.Cost=evaluate(s.Values, constraints);
                }

                
                for (int i = 0; i < mNumChildren; ++i)
                {
                    pop[mPopSize-mNumChildren-1+i] = children[i];
                }

                OnStepped(best_solution, iteration);
                iteration++;
            }

            return best_solution;
        }
    }
}
