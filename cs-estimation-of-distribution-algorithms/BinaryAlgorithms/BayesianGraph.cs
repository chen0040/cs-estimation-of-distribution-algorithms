using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDA;

namespace EDA.BinaryAlgorithms
{
    public class BayesianGraph
    {
        protected BayesianNode[] mNodes = null;
        public BayesianGraph(int vertex_count)
        {
            mNodes = new BayesianNode[vertex_count];
            for (int i = 0; i < vertex_count; ++i)
            {
                mNodes[i] = new BayesianNode(this);
            }
        }

        public bool PathExists(int i, int j)
        {
            HashSet<int> open_set = new HashSet<int>();
            HashSet<int> closed_set=new HashSet<int>();

            open_set.Add(i);
            while (open_set.Count > 0)
            {
                int k = open_set.First();
                closed_set.Add(k);
                open_set.Remove(k);

                if (k == j)
                {
                    return true;
                }

                foreach (int kk in mNodes[k].OutNodes)
                {
                    if(!closed_set.Contains(kk))
                    {
                        open_set.Add(kk);
                    }
                }
            }

            return false;
        }

        public bool CanAddEdge(int i, int j)
        {
            return !mNodes[i].OutNodes.Contains(j) && PathExists(i, j);
        }

        public HashSet<int> GetViableParents(int i)
        {
            HashSet<int> viable_parents = new HashSet<int>();
            for (int v = 0; v < mNodes.Length; ++v)
            {
                if (CanAddEdge(v, i))
                {
                    viable_parents.Add(v);
                }
            }

            return viable_parents;
        }

        public int[] ComputeCountsForEdges(IEnumerable<BinarySolution> pop, List<int> indexes)
        {
            int[] counts = new int[(int)(System.Math.Pow(2, indexes.Count))];
            int[] reverse_indexes = new int[indexes.Count];
            
            for (int i = indexes.Count - 1; i >= 0; i--)
            {
                reverse_indexes[indexes.Count - i - 1] = indexes[i];
            }

            foreach (BinarySolution s in pop)
            {
                int index = 0;
                for (int i = 0; i < reverse_indexes.Length; ++i)
                {
                    int v=reverse_indexes[i];
                    index += (s[v] == 1 ? 1 : 0) * ((int)System.Math.Pow(2, i));
                }
                counts[index]+=1;
            }

            return counts;
        }

        public int Factorial(int n)
        {
            if (n <= 1)
            {
                return 1;
            }
            return n * Factorial(n - 1);
        }

        protected double k2equation(int node_id, List<int> candidate, IEnumerable<BinarySolution> pop)
        {
            List<int> candidate_plus1=new List<int>();
            candidate_plus1.Add(node_id);
            candidate_plus1.AddRange(candidate);
            

            int[] counts = ComputeCountsForEdges(pop, candidate_plus1);

            int count2 = counts.Length / 2; 

            double total=0;
            for(int i=0; i < count2; ++i)
            {
                int a1=counts[i * 2];
                int a2=counts[i * 2 + 1];
                total += (1.0 / Factorial(a1+a2 +1) ) * Factorial(a1) * Factorial(a2);
            }
            return total;
        }

        public int Size
        {
            get { return mNodes.Length; }
        }

        protected double[] ComputeGains(int node_id, IEnumerable<BinarySolution> pop, int max = 2)
        {
            int graph_size = Size;

            BayesianNode node = mNodes[node_id];

            HashSet<int> viable_parents = GetViableParents(node_id);

            double[] gains = new double[graph_size];
            for (int i = 0; i < graph_size; ++i)
            {
                if (mNodes[i].InNodes.Count < max && viable_parents.Contains(i))
                {
                    List<int> candidates=node.InNodes.ToList();
                    candidates.Add(i);
                    gains[i] = k2equation(node_id, candidates, pop);
                }
            }

            return gains;
        }

        public void ConstructNetwork(IEnumerable<BinarySolution> pop, int max_edges = -1)
        {
            if (max_edges == -1) max_edges = pop.Count() * 3;

            for (int i = 0; i < max_edges; ++i)
            {
                double max = -1;
                int from = -1;
                int to = -1;

                for (int node1_id = 0; node1_id < Size; ++node1_id)
                {
                    double[] gains = ComputeGains(node1_id, pop);
                    for (int node2_id = 0; node2_id < Size; ++node2_id)
                    {
                        if (gains[node2_id] > max)
                        {
                            max = gains[node2_id];
                            from = node1_id;
                            to = node2_id;
                        }
                    }
                }

                if (max <= 0.0) break;

                mNodes[from].OutNodes.Add(to);
                mNodes[to].InNodes.Add(from);
            }

        }

        public List<int> CreateTopologicalOrder()
        {
            int graph_size = Size;

            Stack<int> stack = new Stack<int>();
            int[] counts = new int[graph_size];
            for(int node_id =0; node_id < Size; ++node_id)
            {
                int count=mNodes[node_id].InNodes.Count;
                if (count == 0)
                {
                    stack.Push(node_id);
                }
                counts[node_id] = count;
            }

            List<int> ordered = new List<int>();

            while (ordered.Count < graph_size)
            {
                int current = stack.Pop();

                BayesianNode current_node = mNodes[current];

                foreach (int out_node_id in current_node.OutNodes)
                {
                    counts[out_node_id] -= 1;
                    if (counts[out_node_id] <= 0)
                    {
                        stack.Push(out_node_id);
                    }
                }

                ordered.Add(current);
            }

            return ordered;
        }

        protected double CalcMarginalProbability(IEnumerable<BinarySolution> pop, int i)
        {
            double sum = 0.0;
            foreach (BinarySolution s in pop)
            {
                sum += s[i];
            }

            return sum; 
        }

        public double CalcProbablity(int node_id, BinarySolution s, IEnumerable<BinarySolution> pop)
        {
            BayesianNode node = mNodes[node_id];
            if (node.InNodes.Count == 0)
            {
                return CalcMarginalProbability(pop, node_id);
            }

            List<int> set = new List<int>();
            set.Add(node_id);
            set.AddRange(node.InNodes);

            int[] counts = ComputeCountsForEdges(pop, set);

            List<int> reverse_in_set = new List<int>();
            for (int i = node.InNodes.Count - 1; i >= 0; i--)
            {
                reverse_in_set[node.InNodes.Count - 1 - i] = node.InNodes[i];
            }

            int index = 0;
            for (int i = 0; i < reverse_in_set.Count; ++i)
            {
                int v = reverse_in_set[i];
                index += (s[v] == 1 ? 1 : 0) * ((int)System.Math.Pow(2, i));
            }

            int i1 = (int)(index + 1 * System.Math.Pow(2, node.InNodes.Count));
            int i2 = (int)(index + 0 * System.Math.Pow(2, node.InNodes.Count));

            int a1=counts[i1];
            int a2=counts[i2];

            double prob = (double)a1 / (a1 + a2);

            return prob;
        }

        public BinarySolution Sample(IEnumerable<BinarySolution> pop)
        {
            int graph_size = Size;
            int[] x = new int[graph_size];
            BinarySolution s=new BinarySolution(x, double.MaxValue);
            for (int v = 0; v < graph_size; ++v)
            {
                double prob = CalcProbablity(v, s, pop);
                if (RandomEngine.NextDouble() < prob)
                {
                    s[v] = 1;
                }
                else
                {
                    s[v] = 0;
                }
            }

            return s;
        }

        public BinarySolution[] Sample(IEnumerable<BinarySolution> pop, int num_samples)
        {
            BinarySolution[] samples = new BinarySolution[num_samples];
            for (int i = 0; i < num_samples; ++i)
            {
                samples[i] = Sample(pop);
            }

            return samples;
        }
    }
}
