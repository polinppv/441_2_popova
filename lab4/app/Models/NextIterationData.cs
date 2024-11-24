namespace app.Models
{   
    public class NextIterationData
    {
        public int best_fitness {get; set;}
        public int[]? best_gen {get; set;}
        public int[][]? population_genes {get; set;}
        public int[]? population_fitnesses {get; set;}
    }
}