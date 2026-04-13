using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;

namespace diary_app.Services;

public class SpeechService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string ElevenLabsApiUrl = "https://api.elevenlabs.io/v1/speech-to-text";
    private const string ApiKey = "sk_7fe081d5c37f743e5ed92e1fc7eca8fbe62b7076ba98f9d2";

    public SpeechService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string?> TranscribeAudioAsync(string base64Audio)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("Speech");
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("xi-api-key", ApiKey);

            var audioBytes = Convert.FromBase64String(base64Audio);
            
            using var content = new MultipartFormDataContent();
            var audioContent = new ByteArrayContent(audioBytes);
            audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/webm");
            
            content.Add(audioContent, "file", "audio.webm");
            content.Add(new StringContent("scribe_v1"), "model_id");

            var response = await httpClient.PostAsync(ElevenLabsApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"ElevenLabs transcription failed: {error}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ElevenLabsTranscriptResponse>();
            return result?.text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ElevenLabs transcription error: {ex.Message}");
            return null;
        }
    }
}

public class ElevenLabsTranscriptResponse
{
    public string? text { get; set; }
}
