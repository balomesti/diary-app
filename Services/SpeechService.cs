using System.Net.Http.Json;
using System.Text.Json;

namespace diary_app.Services;

public class SpeechService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string AssemblyAiApiUrl = "https://api.assemblyai.com/v2";
    private const string ApiKey = "33f38c56a495467d860776f5c2b13034";

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
            httpClient.DefaultRequestHeaders.Add("authorization", ApiKey);

            var audioBytes = Convert.FromBase64String(base64Audio);
            var content = new ByteArrayContent(audioBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var uploadResponse = await httpClient.PostAsync($"{AssemblyAiApiUrl}/upload", content);

            if (!uploadResponse.IsSuccessStatusCode)
            {
                var error = await uploadResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"AssemblyAI upload failed: {error}");
                return null;
            }

            var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<UploadResponse>();
            if (uploadResult == null || string.IsNullOrEmpty(uploadResult.upload_url))
            {
                Console.WriteLine("Failed to get upload URL from AssemblyAI");
                return null;
            }

            var transcriptionRequest = new
            {
                audio_url = uploadResult.upload_url,
                language_code = "en"
            };

            var transcriptResponse = await httpClient.PostAsJsonAsync($"{AssemblyAiApiUrl}/transcript", transcriptionRequest);

            if (!transcriptResponse.IsSuccessStatusCode)
            {
                var error = await transcriptResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"AssemblyAI transcription request failed: {error}");
                return null;
            }

            var transcriptResult = await transcriptResponse.Content.ReadFromJsonAsync<TranscriptResponse>();
            if (transcriptResult == null || string.IsNullOrEmpty(transcriptResult.id))
            {
                Console.WriteLine("Failed to get transcript ID from AssemblyAI");
                return null;
            }

            var status = transcriptResult.status;
            var maxAttempts = 30;
            var attempts = 0;

            while (status != "completed" && status != "error" && attempts < maxAttempts)
            {
                await Task.Delay(1000);
                var statusResponse = await httpClient.GetAsync($"{AssemblyAiApiUrl}/transcript/{transcriptResult.id}");
                if (statusResponse.IsSuccessStatusCode)
                {
                    transcriptResult = await statusResponse.Content.ReadFromJsonAsync<TranscriptResponse>();
                    status = transcriptResult?.status ?? "error";
                }
                attempts++;
            }

            if (transcriptResult?.status == "completed")
            {
                return transcriptResult.text;
            }
            else if (transcriptResult?.status == "error")
            {
                Console.WriteLine($"AssemblyAI transcription error: {transcriptResult.error}");
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Speech transcription error: {ex.Message}");
            return null;
        }
    }

    private class UploadResponse
    {
        public string? upload_url { get; set; }
    }

    private class TranscriptResponse
    {
        public string? id { get; set; }
        public string? status { get; set; }
        public string? text { get; set; }
        public string? error { get; set; }
    }
}