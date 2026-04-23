using System.Net.Http;
using System.Text;
using System.Text.Json;

public class PythonService
{
    private readonly HttpClient _http;

    public PythonService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<int>> GetRecipeIds(List<string> ingredients)
    {
        var body = new
        {
            ingredients = ingredients,
            top_k = 10
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("http://127.0.0.1:8000/recommend", content);

        var result = await response.Content.ReadAsStringAsync();

        var data = JsonSerializer.Deserialize<RecommendResponse>(result);

        return data.recipe_ids;
    }
}