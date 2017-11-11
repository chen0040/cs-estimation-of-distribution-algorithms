# cs-estimation-of-distribution-algorithms

Estimation of Distribution Algorithms implemented in C#

# Features 

The current library support optimization problems in which solutions are either binary-coded or continuous vectors. The algorithms implemented for estimation-of-distribution are listed below:

* PBIL
* CGA (Compact Genetic Algorithm)
* BOA (Bayesian Optimization Algorithm)
* UMDA (Univariate Marginal Distribution Algorithm)
* Cross Entropy Method
* MMIC

# Usage

## Solving Continuous Optimization 

### Running PBIL 

The sample codes below shows how to solve the "Rosenbrock Saddle" continuous optmization problem using PBIL:

```cs
CostFunction_RosenbrockSaddle f = new CostFunction_RosenbrockSaddle();
            
int popSize = 8000;
PBIL s = new PBIL(popSize, f);

s.SolutionUpdated += (best_solution, step) =>
{
	Console.WriteLine("Step {0}: Fitness = {1}", step, best_solution.Cost);
};

int max_iterations = 200;
s.Minimize(f, max_iterations);
```

Where the CostFunction_RosenbrockSaddle is the cost function that is defined as below:

```cs
public class CostFunction_RosenbrockSaddle : CostFunction
{
	public CostFunction_RosenbrockSaddle()
		: base(2, -2.048, 2.048) // 2 is the dimension of the continuous solution, -2.048 and 2.048 is the lower and upper bounds for the two dimensions 
	{

	}

	protected override void _CalcGradient(double[] solution, double[] grad) // compute the search gradent given the solution 
	{
		double x0 = solution[0];
		double x1 = solution[1];
		grad[0] = 400 * (x0 * x0 - x1) * x0 - 2 * (1 - x0);
		grad[1] = -200 * (x0 * x0 - x1);
	}

	// Optional: if not overriden, the default gradient esimator will be provided for gradient computation
	protected override double _Evaluate(double[] solution) // compute the cost of problem given the solution 
	{
		double x0 = solution[0];
		double x1 = solution[1];

		double cost =100 * Math.Pow(x0 * x0 - x1, 2) + Math.Pow(1 - x0, 2);
		return cost;
	}

}
```

### Running CGA

The sample codes below shows how to solve the "Rosenbrock Saddle" continuous optmization problem using CGA:

```cs
CostFunction_RosenbrockSaddle f = new CostFunction_RosenbrockSaddle();
            
int n = 1000; // sample size for the distribution 
CGA s = new CGA(n, f);

s.SolutionUpdated += (best_solution, step) =>
{
	Console.WriteLine("Step {0}: Fitness = {1}", step, best_solution.Cost);
};

int max_iterations = 2000000;
s.Minimize(f, max_iterations);
```

### Running UMDA

The sample codes below shows how to solve the "Rosenbrock Saddle" continuous optmization problem using UMDA:

```cs
CostFunction_RosenbrockSaddle f = new CostFunction_RosenbrockSaddle();
            
int popSize = 1000; 
int selectionSize = 100;
UMDA s = new UMDA(popSize, selectionSize, f);

s.SolutionUpdated += (best_solution, step) =>
{
	Console.WriteLine("Step {0}: Fitness = {1}", step, best_solution.Cost);
};

int max_iterations = 2000000;
s.Minimize(f, max_iterations);
```

### Running CrossEntropyMethod

The sample codes below shows how to solve the "Rosenbrock Saddle" continuous optmization problem using CrossEntropyMethod:

```cs
CostFunction_RosenbrockSaddle f = new CostFunction_RosenbrockSaddle();
            
int sampleSize = 1000; 
int selectionSize = 100;
CrossEntropyMethod s = new CrossEntropyMethod(sampleSize, selectionSize, f);

s.SolutionUpdated += (best_solution, step) =>
{
	Console.WriteLine("Step {0}: Fitness = {1}", step, best_solution.Cost);
};

int max_iterations = 2000000;
s.Minimize(f, max_iterations);
```

## Solving Problems with Binary-encoded Solutions 

### Running PBIL 

The samle codes below show how to solve a canonical optimization problem that look for solutions with minimum number of 1 bits in the solution:

```cs 
int popSize = 8000;
int dimension = 50;
int eliteCount = 50;
PBIL s = new PBIL(popSize, dimension, eliteCount);
s.MaxIterations = 100;

s.SolutionUpdated += (best_solution, step) =>
{
	Console.WriteLine("Step {0}: Fitness = {1}", step, best_solution.Cost);
};

s.Minimize((solution, constraints) =>
{
	// solution is binary-encoded
	double cost = 0;
	// minimize the number of 1 bits in the solution
	for(int i=0; i < solution.Length; ++i)
	{
		cost += solution[i]; 
	}
	return cost;
});
```

### Running CGA 

The samle codes below show how to solve a canonical optimization problem that look for solutions with minimum number of 1 bits in the solution:

```cs 
int sampleSize = 8000;
int dimension = 50;
int sampleSelectionSize = 100;
CGA s = new CGA(sampleSize, dimension, sampleSelectionSize);
s.MaxIterations = 100;

s.SolutionUpdated += (best_solution, step) =>
{
	Console.WriteLine("Step {0}: Fitness = {1}", step, best_solution.Cost);
};

s.Minimize((solution, constraints) =>
{
	// solution is binary-encoded
	double cost = 0;
	// minimize the number of 1 bits in the solution
	for(int i=0; i < solution.Length; ++i)
	{
		cost += solution[i]; 
	}
	return cost;
});
```

### Running UMDA 

The samle codes below show how to solve a canonical optimization problem that look for solutions with minimum number of 1 bits in the solution:

```cs 
int sampleSize = 8000;
int dimension = 50;
int sampleSelectionSize = 100;
UMDA s = new UMDA(sampleSize, dimension, sampleSelectionSize);
s.MaxIterations = 100;

s.SolutionUpdated += (best_solution, step) =>
{
	Console.WriteLine("Step {0}: Fitness = {1}", step, best_solution.Cost);
};

s.Minimize((solution, constraints) =>
{
	// solution is binary-encoded
	double cost = 0;
	// minimize the number of 1 bits in the solution
	for(int i=0; i < solution.Length; ++i)
	{
		cost += solution[i]; 
	}
	return cost;
});
```

