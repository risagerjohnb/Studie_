using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Studie_.Services;

public sealed class RandomUserApiClient(HttpClient httpClient, IConfiguration configuration)
{
    public async Task<IReadOnlyList<RandomUserProfile>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = configuration["ApiNinjas:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "missing api key");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://api.api-ninjas.com/v2/randomuser?count=6&fields=name,email,job,company,avatar");
        request.Headers.TryAddWithoutValidation("X-Api-Key", apiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<RandomUserProfile>>(
            cancellationToken: cancellationToken);

        return users ?? throw new JsonException("The random-user API returned an empty response.");
    }
}

public sealed class RandomUserProfile
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }
}
