using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using app.Models;
using Microsoft.AspNetCore.SignalR;

namespace Lab4
{
    namespace app.Controllers
    {
        public class HomeController : Controller
        {
            private int[] square_sizes = null!;
            private int LENGTH_CHROM;
            private int POLE_SIZE;
            private int SQUARES;
            private Individual[] population = null!;
            private int[] ideal_gen = null!;
            private CancellationTokenSource cancellationTokenSource = null!;
            private int StartGeneration = 0;
            private int Ideal_fitness = -1;
            private int population_size = 500;
            private int num1x1 = 3;
            private int num2x2 = 2;
            private int num3x3 = 1;
            private CancellationToken token;
            private readonly IHubContext<GeneticAlgorithmHub> _hubContext;
            public HomeController(IHubContext<GeneticAlgorithmHub> hubContext)
            {
                _hubContext = hubContext;
            }
            public IActionResult Index()
            {
                return View();
            }
            public IActionResult Privacy()
            {
                return View();
            }
            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error()
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
            [HttpPost("next")]
            public IActionResult NextIteration([FromBody] Initial data)
            {
                num1x1 = data.n1x1_;
                num2x2 = data.n2x2_;
                num3x3 = data.n3x3_;
                SQUARES = num1x1 + num2x2 + num3x3;
                LENGTH_CHROM = 2 * SQUARES;
                POLE_SIZE = (int)Math.Sqrt(num1x1 + num2x2 * 4 + num3x3 * 9) * 3;
                square_sizes = 
                    Enumerable.Range(0, num1x1).Select(_ => 1).Concat(
                        Enumerable.Range(0, num2x2).Select(_ => 2).Concat(
                            Enumerable.Range(0, num3x3).Select(_ => 3))).ToArray();
                population = new Individual[population_size];
                var population_genes_ = new int[population_size][];
                for (var i = 0; i < population_size; i++)
                    population_genes_[i] = new int[LENGTH_CHROM];
                population_genes_ = data.population_genes;
                var population_fitnesses_ = new int[population_size];
                population_fitnesses_ = data.population_fitnesses;
                for (var i = 0; i < population_size; i++)
                {
                    population[i] = new Individual(LENGTH_CHROM, POLE_SIZE);
                    population[i].genes = population_genes_![i];
                    population[i].fitness = population_fitnesses_![i];
                }
                Individual[] offspring = GeneticAlgo.SelTournament(population, population.Length);
                for (int i = 0; i < offspring.Length; i += 2)
                    if (new Random().NextDouble() < 0.9)
                    {
                        var children = GeneticAlgo.Crossover(offspring[i], offspring[i + 1]);
                        offspring[i] = children.Item1;
                        offspring[i + 1] = children.Item2;
                    }
                for (int i = 0; i < offspring.Length; i++)
                    if (new Random().NextDouble() < 0.4)
                        offspring[i].Mutate(1.0 / LENGTH_CHROM, POLE_SIZE);
                foreach (Individual ind in offspring)
                    ind.fitness = ind.Loss(square_sizes, POLE_SIZE);
                population = offspring;
                int minFitness = population.Min(ind => ind.fitness);
                foreach (Individual ind in population)
                    if (ind.fitness == minFitness) 
                    {
                        ideal_gen = ind.genes;
                        Ideal_fitness = minFitness;
                        break;
                    }
                var current_population_genes = new int[population_size][];
                var current_population_fitnesses = new int[population_size];
                for (var i = 0; i < population_size; i++)
                {
                    current_population_genes[i] = new int[LENGTH_CHROM];
                    current_population_genes[i] = population[i].genes;
                    current_population_fitnesses[i] = population[i].fitness;
                }
                var response = new NextIterationData
                {
                    best_fitness = Ideal_fitness,
                    best_gen = ideal_gen,
                    population_fitnesses = current_population_fitnesses,
                    population_genes = current_population_genes
                };
                return Ok(response);
            }
            [HttpGet("initial")]
            public IActionResult InitialPopulation(int n1x1=3, int n2x2=2, int n3x3=3)
            {
                num1x1 = n1x1;
                num2x2 = n2x2;
                num3x3 = n3x3;
                SQUARES = num1x1 + num2x2 + num3x3;
                LENGTH_CHROM = 2 * SQUARES;
                POLE_SIZE = (int)Math.Sqrt(num1x1 + num2x2 * 4 + num3x3 * 9) * 3;
                square_sizes = 
                    Enumerable.Range(0, num1x1).Select(_ => 1).Concat(
                        Enumerable.Range(0, num2x2).Select(_ => 2).Concat(
                            Enumerable.Range(0, num3x3).Select(_ => 3))).ToArray();
                population = GeneticAlgo.PopulationCreator(population_size, LENGTH_CHROM, POLE_SIZE);
                var current_population_genes = new int[population_size][];
                var current_population_fitnesses = new int[population_size];
                for (var i = 0; i < population_size; i++)
                {
                    current_population_genes[i] = new int[LENGTH_CHROM];
                    current_population_genes[i] = population[i].genes;
                    current_population_fitnesses[i] = population[i].fitness;
                }
                var userInfo = new Initial
                {
                    n1x1_ = n1x1,
                    n2x2_ = n2x2,
                    n3x3_ = n3x3,
                    population_genes = current_population_genes,
                    population_fitnesses = current_population_fitnesses,
                };
                return Ok(userInfo);
            }
            [HttpPost]
            [Route("Start")]
            public async Task<IActionResult> Start([FromBody] SquareSizes json_file)
            {   
                num1x1 = json_file.n1x1;
                num2x2 = json_file.n2x2;
                num3x3 = json_file.n3x3;
                Ideal_fitness = -1;
                SQUARES = num1x1 + num2x2 + num3x3;
                LENGTH_CHROM = 2 * SQUARES;
                POLE_SIZE = (int)Math.Sqrt(num1x1 + num2x2 * 4 + num3x3 * 9) * 3;
                square_sizes = 
                    Enumerable.Range(0, num1x1).Select(_ => 1).Concat(
                        Enumerable.Range(0, num2x2).Select(_ => 2).Concat(
                            Enumerable.Range(0, num3x3).Select(_ => 3))).ToArray();
                population = GeneticAlgo.PopulationCreator(population_size, LENGTH_CHROM, POLE_SIZE);
                cancellationTokenSource = new CancellationTokenSource();
                token = cancellationTokenSource.Token;
                await Task.Factory.StartNew(async () =>
                {
                    for (int generation = StartGeneration; generation < 1000; generation++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }
                        Individual[] offspring = GeneticAlgo.SelTournament(population, population.Length);
                        for (int i = 0; i < offspring.Length; i += 2)
                            if (new Random().NextDouble() < 0.9)
                            {
                                var children = GeneticAlgo.Crossover(offspring[i], offspring[i + 1]);
                                offspring[i] = children.Item1;
                                offspring[i + 1] = children.Item2;
                            }
                        for (int i = 0; i < offspring.Length; i++)
                            if (new Random().NextDouble() < 0.4)
                                offspring[i].Mutate(1.0 / LENGTH_CHROM, POLE_SIZE);
                        foreach (Individual ind in offspring)
                            ind.fitness = ind.Loss(square_sizes, POLE_SIZE);
                        population = offspring;
                        int minFitness = population.Min(ind => ind.fitness);
                        foreach (Individual ind in population)
                            if (ind.fitness == minFitness) 
                            {
                                ideal_gen = ind.genes;
                                Ideal_fitness = minFitness;
                                break;
                            }
                        var response = new GeneticAlgorithmData
                        {
                            fitness = Ideal_fitness,
                            gen = ideal_gen,
                            length_chrom = LENGTH_CHROM,
                            sqr_sizes = square_sizes,
                            current_generation = generation + 1
                        };
                        if (!token.IsCancellationRequested)
                            await _hubContext.Clients.All.SendAsync("UpdateData", response);
                        else
                            break;
                        Thread.Sleep(3);
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                if (!token.IsCancellationRequested)
                    await _hubContext.Clients.All.SendAsync("ResultData");
                return Ok();
            }
            [HttpPost]
            [Route("Stop")]
            public IActionResult Stop()
            { 
                if (cancellationTokenSource != null)
                    cancellationTokenSource.Cancel();
                return Ok();
            }
        }
    }
}