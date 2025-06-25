    using Microsoft.AspNetCore.Mvc;
    using SlackProfessionalBot.Model.Response;
    using System.Net.Http.Headers;

    [ApiController]
    [Route("api/slack")]
    public class SlackController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public SlackController(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }


    [HttpPost("slash")]
    public async Task<IActionResult> SlashCommand([FromForm] string text) // Slack sends data as form-urlencoded
    {
        // 1. Prepare the prompt for Gemini
        var prompt = $"Convert this into a more professional and diplomatic tone: {text}";

        // 2. Get Gemini API Key from configuration
        var geminiApiKey = _config["Gemini:ApiKey"]; // Ensure you have this in your appsettings.json or environment variables

        if (string.IsNullOrEmpty(geminiApiKey))
        {
            return StatusCode(500, "Gemini API Key is not configured.");
        }

        var client = _httpClientFactory.CreateClient();

        // 3. Construct the request body for Gemini API
        // For Gemini, messages are typically in a 'contents' array,
        // and each message has a 'role' (user/model) and 'parts' array.
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            // Optional: configure generation parameters
            generationConfig = new
            {
                temperature = 0.5,
                maxOutputTokens = 1000 // Equivalent to max_tokens
            },
            // Optional: safety settings
            safetySettings = new[]
            {
                new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" }
            }
        };

        // 4. Send the request to Gemini API
        // The API key is typically sent as a query parameter for Gemini REST API,
        // or as a header depending on the specific client library.
        // For direct REST calls, query parameter is common.
        var geminiApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={geminiApiKey}";

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync(geminiApiUrl, requestBody);
        }
        catch (HttpRequestException ex)
        {
            // Log the exception for debugging
            Console.WriteLine($"HTTP Request Error: {ex.Message}");
            return StatusCode(500, "Could not connect to Gemini API. Please try again later.");
        }


        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Gemini API Error: {response.StatusCode} - {errorContent}");
            return StatusCode(500, $"Gemini API error ({response.StatusCode}). Details: {errorContent}");
        }

        // 5. Parse the Gemini API response
        // Define a simple class to deserialize the response, similar to your OpenAIResponse.
        // The structure for Gemini is different.
        var geminiResult = await response.Content.ReadFromJsonAsync<GeminiResponse>();

        var reply = "Error processing request from Gemini.";
        if (geminiResult?.candidates != null && geminiResult.candidates.Length > 0)
        {
            // Gemini's response for generated text is usually in candidates[0].content.parts[0].text
            reply = geminiResult.candidates[0].content?.parts?[0]?.text?.Trim() ?? reply;
        }

        // 6. Return the Slack response
        return Ok(new
        {
            response_type = "in_channel", // or "ephemeral" for private
            text = $"💼 *Professional Version:*\n{reply}"
        });
    }
}