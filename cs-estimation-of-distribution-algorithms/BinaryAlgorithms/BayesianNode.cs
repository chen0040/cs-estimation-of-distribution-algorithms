using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDA.BinaryAlgorithms
{
    public class BayesianNode
    {
        protected BayesianGraph mGraph;
        public BayesianNode(BayesianGraph g)
        {

        }
        protected int mId;
        public int Id
        {
            get { return mId; }
            set { mId = value; }
        }

        protected List<int> mOutNodes = new List<int>();
        protected List<int> mInNodes = new List<int>();

        public List<int> OutNodes
        {
            get { return mOutNodes; }
        }

        public List<int> InNodes
        {
            get { return mInNodes; }
        }
    }
}
