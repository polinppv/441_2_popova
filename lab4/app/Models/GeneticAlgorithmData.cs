namespace app.Models
{   
    public class GeneticAlgorithmData
    {
        public int fitness {get; set;}
        public int[]? gen {get; set;}
        public int length_chrom {get; set;}
        public int[]? sqr_sizes {get; set;}
        public int current_generation {get; set;}
    }
}