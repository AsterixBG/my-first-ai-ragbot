using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Newtonsoft.Json;
using OllamaSharp;

#pragma warning disable SKEXP0020
#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0001

class Program
{
    static async Task Main()
    {
        // Connecting to Qdrant with the correct vector size (4096)
        var dbClient = new QdrantVectorDbClient("http://localhost:6333", vectorSize: 4096); // 4096 for Mistral
        var memoryStore = new QdrantMemoryStore(dbClient);

        // Setting up HttpClient for Ollama
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };
        // Adding an embedding generator (Ollama)
        var embeddingGenerator = new OllamaApiClient(httpClient, "mistral").AsTextEmbeddingGenerationService();

        // Creating vector memory (in Qdrant)
        var memory = new SemanticTextMemory(memoryStore, embeddingGenerator);

        // Initializing the Kernel with a local AI model
        var kernel = Kernel.CreateBuilder()
            .AddOllamaChatCompletion("mistral", httpClient: httpClient)
            .Build();

        // We importing plugins that make our chatbot more capable
        kernel.ImportPluginFromObject(new WeatherPlugin(), "Weather");

        Console.WriteLine("AI Chatbot (RAG) – Type 'exit' to quit.");

        while (true)
        {
            Console.Write("Enter your question: ");
            string input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input)) continue;
            if (input.ToLower() == "exit") break;

            var history = new ChatHistory();
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            history.AddUserMessage(input);
            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() },
                kernel: kernel);

            history.AddMessage(result.Role, result.Content ?? string.Empty);

            Console.WriteLine($"AI: {result}");
        }

        Console.WriteLine("The chatbot has been stopped.");
    }
}

public class WeatherPlugin
{
    private const string CollectionName = "my-first-ragbot.geo-coords";
    private readonly HttpClient _httpClient;
    // Get your free api key https://openweathermap.org/
    private const string ApiKey = "";
    private readonly Dictionary<string, GeoResponse> _geoResponseCache = new();
    private readonly SemanticTextMemory _memory;


    public WeatherPlugin()
    {
        _httpClient = new HttpClient();
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };
        var embeddingGenerator = new OllamaApiClient(httpClient, "mistral").AsTextEmbeddingGenerationService();
        string qdrantUrl = "http://localhost:6333";
        var memoryStore = new QdrantMemoryStore(qdrantUrl, vectorSize: 4096);

        _memory = new SemanticTextMemory(memoryStore, embeddingGenerator);
    }

    [KernelFunction]
    public async Task<string> GetWeather(string city)
    {
        try
        {
            var geoData = await GetCityCoordinates(city);

            string url = $"https://api.openweathermap.org/data/2.5/weather?lat={geoData.Lat}&lon={geoData.Lon}&appid={ApiKey}&units=metric&lang=bg";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var weatherData = JsonConvert.DeserializeObject<WeatherResponse>(json);

            return $"The weather in {city}: {weatherData.Main.Temp}°C, {weatherData.Weather[0].Description}.";
        }
        catch (Exception ex)
        {
            if (ex.Message == "Missing API key.")
            {
                // We tell the chatbot what the problem is and how the user can resolve it.
                return "OpenWeatherMap API key is missing. The user must set the key and restart the application before trying to ask for the weather again.";
            }
            Console.WriteLine($"[DBUG] Failed to get information from API. {ex.Message}");
            return "Error during weather retrieval.";
        }
    }

    private async Task<GeoResponse> GetCityCoordinates(string city)
    {
        // Check in-memory cache for result
        if (!_geoResponseCache.TryGetValue(city, out GeoResponse result))
        {
            var memoryResults = _memory.SearchAsync(CollectionName, city);
            Console.WriteLine($"[DBUG] Checking memory for {city}");
            // Check in database if someone already looked for that city
            if (await memoryResults.AnyAsync(m => m.Metadata.Text == city))
            {
                Console.WriteLine($"[DBUG] Getting {city} coordinates from memory.");
                var geoCoordsResult = await memoryResults.FirstOrDefaultAsync();
                Console.WriteLine($"[DBUG] Got {geoCoordsResult.Metadata.Text} from memory.");
                result = JsonConvert.DeserializeObject<GeoResponse>(geoCoordsResult.Metadata.Text);
            }
            else // If it's new city go to the API and find it's geo location
            {
                if (string.IsNullOrWhiteSpace(ApiKey))
                {
                    Console.WriteLine("[DBUG] Missing API key. Can't call weather api. Get your api key and try again.");
                    throw new Exception("Missing API key.");
                }
                Console.WriteLine($"[DBUG] Getting {city} from API.");
                string geoUrl = $"https://api.openweathermap.org/geo/1.0/direct?q={city}&appid={ApiKey}&limit=1&units=metric&lang=bg";
                var geoResponse = await _httpClient.GetAsync(geoUrl);
                var geoJson = await geoResponse.Content.ReadAsStringAsync();
                var geoData = JsonConvert.DeserializeObject<IEnumerable<GeoResponse>>(geoJson);
                var cityGeoData = geoData.First();
                var cityJson = JsonConvert.SerializeObject(cityGeoData);
                // Save it in database for future use
                await _memory.SaveInformationAsync(CollectionName, cityJson, city);
                Console.WriteLine($"[DBUG] Got {cityJson} from API.");
                result = cityGeoData;

            }
            // Save it in memory
            _geoResponseCache[city] = result;
        }
        return result;
    }

    private class GeoResponse
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
    private class WeatherResponse
    {
        public WeatherMain Main { get; set; }
        public WeatherInfo[] Weather { get; set; }
    }

    private class WeatherMain
    {
        public float Temp { get; set; }
    }

    private class WeatherInfo
    {
        public string Description { get; set; }
    }
}
