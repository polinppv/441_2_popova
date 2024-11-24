namespace Lab4
{
    public class Individual
    {
        public int[] genes;
        public int Length => genes.Length;
        public int fitness;
        public Individual(int lengthChrom, int poleSize)
        {
            Random random = new Random();
            genes = new int[lengthChrom];
            for (int i = 0; i < lengthChrom; i++)
                genes[i] = random.Next(1, poleSize + 1);
            fitness = 0;
        }
        public Individual(int[] genes)
        {
            this.genes = (int[])genes.Clone();
            fitness = 0;
        } 
        public void Mutate(double mutPb, int poleSize)
        {
            Random random = new Random();
            for (int i = 0; i < genes.Length; i++)
                if (random.NextDouble() < mutPb)
                    genes[i] = random.Next(1, poleSize + 1);
        }
        public Individual Clone()
        {
            Individual clone = new Individual(genes);
            clone.fitness = this.fitness;
            return clone;
        }
        public int Loss(int[] squareSizes, int poleSize)
        {
            int min_x = int.MaxValue, min_y = int.MaxValue, max_x = int.MinValue, max_y = int.MinValue;
            for (int i = 0; i < genes.Length; i += 2)
            {
                int square_x = genes[i], square_y = genes[i + 1], t = squareSizes[i / 2];
                min_x = Math.Min(square_x, min_x);
                min_y = Math.Min(square_y, min_y);
                max_x = Math.Max(square_x + t, max_x);
                max_y = Math.Max(square_y + t, max_y);
            }
            if (Intersect(squareSizes, poleSize)) 
                return int.MaxValue;
            int area = (max_x - min_x) * (max_y - min_y);
            return area;
        }
        private bool Intersect(int[] squareSizes, int poleSize)
        {
            for (int i = 0; i < genes.Length; i += 2)
                for (int j = 0; j < genes.Length; j += 2)
                    if (i != j)
                    {
                        int square1_x = genes[i], square1_y = genes[i + 1], t1 = squareSizes[i / 2];
                        int square2_x = genes[j], square2_y = genes[j + 1], t2 = squareSizes[j / 2];
                        if (!(square1_x >= square2_x + t2 || square1_y >= square2_y + t2 || square2_x >= square1_x + t1 || square2_y >= square1_y + t1))
                            return true;
                    }
            return false;
        }
    }
}