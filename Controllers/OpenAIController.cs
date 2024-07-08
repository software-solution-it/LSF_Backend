using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LSF.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class OpenAIController : ControllerBase
    {
        private readonly string _apiKey;
        private readonly string _assistantId;
        private readonly APIDbContext _dbContext;

        public OpenAIController(APIDbContext dbContext)
        {
            _apiKey = Environment.GetEnvironmentVariable("API_KEY");
            _assistantId = Environment.GetEnvironmentVariable("ASSISTANT_ID");
            _dbContext = dbContext;
        }

        [HttpPost("assistente-lavanderia")]
        public async Task<IActionResult> AssistenteLavanderia([FromBody] InputRequest request)
        {
            if (string.IsNullOrEmpty(request.Input))
            {
                return BadRequest("Input cannot be empty.");
            }

            int userId = await GetUserIdFromSessionOrRequestAsync();
            var threadId = await GetOrCreateThread(userId);
            await AddMessageToThread(threadId, request.Input);
            var response = await RunAssistant(threadId);

            return Ok(response);
        }

        [HttpPost("fechar-chat")]
        public async Task<IActionResult> FecharChat()
        {
            int userId = await GetUserIdFromSessionOrRequestAsync();
            await ClearThreadIdForUser(userId);

            return Ok("Thread has been cleared.");
        }

        private async Task<string> GetOrCreateThread(int userId)
        {
            var existingThreadId = await GetThreadIdForUser(userId);
            if (!string.IsNullOrEmpty(existingThreadId))
            {
                return existingThreadId;
            }

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");

                var response = await httpClient.PostAsync("https://api.openai.com/v1/threads", null);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
                string newThreadId = jsonResponse?.id;

                await StoreThreadIdForUser(userId, newThreadId);

                return newThreadId;
            }
        }

        private async Task AddMessageToThread(string threadId, string input)
        {
            var requestBody = new
            {
                role = "user",
                content = input
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");

                var response = await httpClient.PostAsync($"https://api.openai.com/v1/threads/{threadId}/messages", requestContent);
                response.EnsureSuccessStatusCode();
            }
        }

        private async Task<string> RunAssistant(string threadId)
        {
            var requestBody = new
            {
                assistant_id = _assistantId,
                model = "gpt-4-turbo",
                temperature = 0.2,
                max_completion_tokens = 400,
                top_p = 1.0
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");

                var runResponse = await httpClient.PostAsync($"https://api.openai.com/v1/threads/{threadId}/runs", requestContent);
                runResponse.EnsureSuccessStatusCode();

                var runResponseBody = await runResponse.Content.ReadAsStringAsync();
                var runJsonResponse = JsonConvert.DeserializeObject<JObject>(runResponseBody);
                string runId = runJsonResponse?["id"]?.ToString();

                while (true)
                {
                    var statusResponse = await httpClient.GetAsync($"https://api.openai.com/v1/threads/{threadId}/runs/{runId}");
                    statusResponse.EnsureSuccessStatusCode();

                    var statusResponseBody = await statusResponse.Content.ReadAsStringAsync();
                    var statusJsonResponse = JsonConvert.DeserializeObject<JObject>(statusResponseBody);

                    string status = statusJsonResponse?.Value<string>("status");
                    if (status == "completed" || status == "incomplete")
                    {
                        var messages = await GetMessagesFromThread(threadId);
                        return messages.ToString();
                    }
                    else if (status == "failed")
                    {
                        return "The assistant run failed.";
                    }

                    await Task.Delay(2000);
                }
            }
        }

        private async Task<JArray> GetMessagesFromThread(string threadId)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");

                var response = await httpClient.GetAsync($"https://api.openai.com/v1/threads/{threadId}/messages");
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonConvert.DeserializeObject<JObject>(responseBody);

                var messages = jsonResponse?["data"] as JArray;
                if (messages == null)
                {
                    return new JArray();
                }

                var lastAssistantMessage = messages.OrderByDescending(m => m.Value<long>("created_at"))
                                                   .FirstOrDefault(m => m.Value<string>("role") == "assistant");

                if (lastAssistantMessage == null)
                {
                    return new JArray();
                }

                var assistMessage = lastAssistantMessage["content"]?[0]?["text"]?["value"]?.ToString();
                assistMessage = assistMessage?.Replace("\n", ""); // Remove newline characters

                var result = new JArray
                {
                    new JObject
                    {
                        ["createdAt"] = DateTimeOffset.FromUnixTimeSeconds(lastAssistantMessage.Value<long>("created_at")).ToString("dd/MM/yyyy HH:mm:ss"),
                        ["assistMessage"] = assistMessage,
                        ["order"] = 0
                    }
                };

                return result;
            }
        }

        private async Task<int> GetUserIdFromSessionOrRequestAsync()
        {
            var userId = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            return user.Id;
        }

        private async Task<string> GetThreadIdForUser(int userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            return user?.ThreadId;
        }

        private async Task StoreThreadIdForUser(int userId, string threadId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.ThreadId = threadId;
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task ClearThreadIdForUser(int userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.ThreadId = null;
                await _dbContext.SaveChangesAsync();
            }
        }
    }

    public class InputRequest
    {
        public string Input { get; set; }
    }
}
