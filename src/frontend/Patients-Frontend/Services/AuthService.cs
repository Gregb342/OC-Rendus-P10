public async Task<bool> LoginAsync(LoginDto loginDto)
{
    try
    {
        var json = JsonSerializer.Serialize(loginDto, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Debug : afficher l'URL complète
        var fullUrl = new Uri(_httpClient.BaseAddress, "/auth/login");
        Console.WriteLine($"Tentative de connexion à : {fullUrl}");

        var response = await _httpClient.PostAsync("/auth/login", content);

        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

            if (result.TryGetProperty("token", out var tokenElement))
            {
                _token = tokenElement.GetString();
                AuthenticationStateChanged?.Invoke();
                return true;
            }
        }
        else
        {
            // Debug : afficher le code d'erreur
            Console.WriteLine($"Erreur HTTP : {response.StatusCode}");
        }

        return false;
    }
    catch (Exception ex)
    {
        // Debug : afficher l'exception
        Console.WriteLine($"Exception : {ex.Message}");
        return false;
    }
}