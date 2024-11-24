using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http.Json;
using app.Models;
using Microsoft.AspNetCore.Hosting;
using System.Windows;
using System.Runtime.CompilerServices;
using System.IO;

namespace app
{
    public class Tests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;
        public Tests(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }
        [Fact]
        public async Task postNextOk()
        {
            var client = factory.CreateClient();
            var jsonContent = await File.ReadAllTextAsync("../../../../app/input.json");
            var response = await client.PostAsJsonAsync("http://localhost:5134/next", JsonConvert.DeserializeObject<Initial>(jsonContent));
            Assert.Equal(200, (int)response.StatusCode);
        }
        [Fact]
        public async Task postNextCorrectJSON()
        {
            var client = factory.CreateClient();
            var jsonContent = await File.ReadAllTextAsync("../../../../app/input.json");
            var response = await client.PostAsJsonAsync("http://localhost:5134/next", JsonConvert.DeserializeObject<Initial>(jsonContent));
            var answer = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<NextIterationData>(answer)!;
            Assert.Equal(200, (int)response.StatusCode);
            Assert.Equal(json.population_fitnesses!.Min(), json.best_fitness);
            Assert.Equal(500, json.population_genes!.Length);
        }
        [Fact]
        public async Task postNextIncorrect()
        {
            var client = factory.CreateClient();
            var jsonContent = await File.ReadAllTextAsync("../../../../app/input.json");
            var response = await client.PostAsJsonAsync("http://localhost:5134/qwerty", JsonConvert.DeserializeObject<Initial>(jsonContent));
            Assert.Equal(404, (int)response.StatusCode);
        }
        [Fact]
        public async Task getInitial()
        {
            var client = factory.CreateClient();
            var response = await client.GetAsync("http://localhost:5134/initial?n1x1=3&n2x2=3&n3x3=10");
            Assert.Equal(200, (int)response.StatusCode);
        }
        [Fact]
        public async Task getInitialCheckBody()
        {
            var client = factory.CreateClient();
            var response = await client.GetAsync("http://localhost:5134/initial?n1x1=3&n2x2=3&n3x3=10");
            var answer = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<Initial>(answer);
            Assert.NotNull(json);
            Assert.Equal(16, json.n1x1_ + json.n2x2_ + json.n3x3_);
            Assert.Equal(200, (int)response.StatusCode);
        }
        [Fact]
        public async Task getInitialCheckDefaultValues()
        {
            var client = factory.CreateClient();
            var response = await client.GetAsync("http://localhost:5134/initial?");
            var answer = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<Initial>(answer)!;
            Assert.Equal(3, json.n1x1_);
            Assert.Equal(2, json.n2x2_);
            Assert.Equal(3, json.n3x3_);
            Assert.Equal(200, (int)response.StatusCode);
        }
    }
}