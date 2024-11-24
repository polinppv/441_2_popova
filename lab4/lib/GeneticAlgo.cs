namespace Lab4
{
    public class GeneticAlgo
    {
        public static Individual[] PopulationCreator(int n, int chrom, int pole)
        {
            Individual[] population = new Individual[n];
            for (int i = 0; i < n; i++)
                population[i] = new Individual(chrom, pole);
            return population;
        }
        public static Individual[] SelTournament(Individual[] population, int size)
        {
            Random random = new Random();
            Individual[] offspring = new Individual[size];
            for (int n = 0; n < size; n++)
            {
                int i1 = random.Next(size);
                int i2 = random.Next(size);
                int i3 = random.Next(size);
                while (i1 == i2 || i1 == i3 || i2 == i3)
                {
                    i1 = random.Next(size);
                    i2 = random.Next(size);
                    i3 = random.Next(size);
                }
                var selected = new[] {population[i1], population[i2], population[i3]}
                                .OrderBy(ind => ind.fitness).First();
                offspring[n] = selected.Clone();
            }
            return offspring;
        }
        public static Tuple<Individual, Individual> Crossover(Individual ind1, Individual ind2)
        {
            Random random = new Random();
            int size = ind1.Length;
            int point1 = random.Next(1, size); 
            int point2 = random.Next(point1 + 1, size + 1); 
            for (int i = point1; i < point2; i++)
            {
                int tempGene = ind1.genes[i];
                ind1.genes[i] = ind2.genes[i];
                ind2.genes[i] = tempGene;
            }
            return Tuple.Create(ind1, ind2);
        }
    }
}