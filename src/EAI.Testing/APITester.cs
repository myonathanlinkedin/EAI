using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;

namespace EAI.Testing
{
    public class APITester
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public APITester(string baseUrl = "https://localhost:58080")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(120); // Increased timeout for LLM processing
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task<TestResult> TestApprovalEndpointAsync(ApprovalRequestDto request)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                Console.WriteLine($"🔄 Testing request: {request.RequestType} - ${request.Amount} - {request.Department}");
                
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/approval/process", content);
                stopwatch.Stop();
                
                Console.WriteLine($"📡 Response Status: {response.StatusCode} (took {stopwatch.ElapsedMilliseconds}ms)");
                
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📄 Raw Response: {responseContent}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<DecisionOutputDto>(responseContent, _jsonOptions);
                    
                    Console.WriteLine($"✅ Parsed Result:");
                    Console.WriteLine($"   Decision: {result?.Decision}");
                    Console.WriteLine($"   Confidence: {result?.ConfidenceScore}");
                    Console.WriteLine($"   Reasoning: {result?.Reasoning}");
                    
                    return new TestResult
                    {
                        Success = true,
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        Decision = result?.Decision ?? "unknown",
                        Confidence = result?.ConfidenceScore ?? 0,
                        Reasoning = result?.Reasoning ?? "no reasoning provided"
                    };
                }
                else
                {
                    Console.WriteLine($"❌ Error Response: {responseContent}");
                    return new TestResult
                    {
                        Success = false,
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        Error = $"HTTP {response.StatusCode}: {responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"💥 Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                return new TestResult
                {
                    Success = false,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Error = ex.Message
                };
            }
        }

        public async Task<TestResult> TestHealthEndpointAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                Console.WriteLine($"🏥 Testing health endpoint...");
                
                var response = await _httpClient.GetAsync($"{_baseUrl}/health");
                stopwatch.Stop();
                
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📡 Health Response: {response.StatusCode} - {content} (took {stopwatch.ElapsedMilliseconds}ms)");
                
                return new TestResult
                {
                    Success = response.IsSuccessStatusCode,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Decision = content
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"💥 Health check exception: {ex.Message}");
                
                return new TestResult
                {
                    Success = false,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Error = ex.Message
                };
            }
        }

        public async Task<List<TestResult>> RunLoadTestAsync(int concurrentUsers, int requestsPerUser)
        {
            var results = new List<TestResult>();
            var testRequests = GenerateTestRequests(concurrentUsers * requestsPerUser);

            // Sequential testing with delays to prevent overwhelming LM Studio
            for (int i = 0; i < concurrentUsers; i++)
            {
                for (int j = 0; j < requestsPerUser; j++)
                {
                    var requestIndex = i * requestsPerUser + j;
                    if (requestIndex < testRequests.Count)
                    {
                        var result = await TestApprovalEndpointAsync(testRequests[requestIndex]);
                        results.Add(result);
                        
                        // Add delay between requests to prevent overwhelming the system
                        if (requestIndex < testRequests.Count - 1)
                        {
                            await Task.Delay(2000); // 2 second delay between requests
                        }
                    }
                }
            }

            return results;
        }

        public async Task<BenchmarkResult> RunComprehensiveBenchmarkAsync()
        {
            Console.WriteLine("🚀 Starting Comprehensive EAI Benchmark...");
            
            // Test 1: Health Check
            Console.WriteLine("📊 Testing Health Endpoint...");
            var healthResult = await TestHealthEndpointAsync();
            
            // Test 2: Single Request Performance
            Console.WriteLine("📊 Testing Single Request Performance...");
            var singleRequest = GenerateTestRequests(1).First();
            var singleResult = await TestApprovalEndpointAsync(singleRequest);
            
            // Test 3: Load Testing - Reduced to prevent overwhelming LM Studio
            Console.WriteLine("📊 Running Load Tests...");
            var loadTestResults = await RunLoadTestAsync(2, 3); // 2 users, 3 requests each (6 total)
            
            // Test 4: Different Request Types
            Console.WriteLine("📊 Testing Different Request Types...");
            var expenseRequest = new ApprovalRequestDto
            {
                RequestType = "expense",
                RequesterId = "user001",
                Amount = 500,
                Description = "Client dinner",
                Department = "Sales",
                Priority = "normal"
            };
            
            var leaveRequest = new ApprovalRequestDto
            {
                RequestType = "leave",
                RequesterId = "user002",
                Days = 7,
                Description = "Vacation request",
                Department = "Engineering",
                Priority = "normal"
            };
            
            var purchaseRequest = new ApprovalRequestDto
            {
                RequestType = "purchase",
                RequesterId = "user003",
                Amount = 2500,
                Description = "Software license",
                Department = "IT",
                Priority = "high"
            };
            
            var expenseResult = await TestApprovalEndpointAsync(expenseRequest);
            var leaveResult = await TestApprovalEndpointAsync(leaveRequest);
            var purchaseResult = await TestApprovalEndpointAsync(purchaseRequest);
            
            var allResults = new List<TestResult> { singleResult, expenseResult, leaveResult, purchaseResult };
            allResults.AddRange(loadTestResults);
            
            return new BenchmarkResult
            {
                HealthCheck = healthResult,
                SingleRequest = singleResult,
                LoadTestResults = loadTestResults,
                RequestTypeResults = new Dictionary<string, TestResult>
                {
                    ["expense"] = expenseResult,
                    ["leave"] = leaveResult,
                    ["purchase"] = purchaseResult
                },
                Summary = CalculateSummary(allResults)
            };
        }

        private List<ApprovalRequestDto> GenerateTestRequests(int count)
        {
            var requests = new List<ApprovalRequestDto>();
            var random = new Random();
            var requestTypes = new[] { "expense", "leave", "purchase" };
            var departments = new[] { "Sales", "Engineering", "Marketing", "HR", "Finance" };
            var priorities = new[] { "low", "normal", "high" };
            
            for (int i = 0; i < count; i++)
            {
                var requestType = requestTypes[random.Next(requestTypes.Length)];
                var request = new ApprovalRequestDto
                {
                    RequestType = requestType,
                    RequesterId = $"user{random.Next(1000, 9999)}",
                    Description = $"Test request {i + 1}",
                    Department = departments[random.Next(departments.Length)],
                    Priority = priorities[random.Next(priorities.Length)]
                };
                
                if (requestType == "expense" || requestType == "purchase")
                {
                    request.Amount = random.Next(50, 5000);
                }
                else if (requestType == "leave")
                {
                    request.Days = random.Next(1, 20);
                }
                
                requests.Add(request);
            }
            
            return requests;
        }

        private BenchmarkSummary CalculateSummary(List<TestResult> results)
        {
            var successfulResults = results.Where(r => r.Success).ToList();
            
            return new BenchmarkSummary
            {
                TotalRequests = results.Count,
                SuccessfulRequests = successfulResults.Count,
                FailedRequests = results.Count - successfulResults.Count,
                SuccessRate = successfulResults.Count / (double)results.Count * 100,
                AverageResponseTime = successfulResults.Any() ? successfulResults.Average(r => r.ResponseTime) : 0,
                MinResponseTime = successfulResults.Any() ? successfulResults.Min(r => r.ResponseTime) : 0,
                MaxResponseTime = successfulResults.Any() ? successfulResults.Max(r => r.ResponseTime) : 0,
                AverageConfidence = successfulResults.Any() ? successfulResults.Where(r => r.Confidence > 0).Average(r => r.Confidence) : 0,
                DecisionDistribution = successfulResults.GroupBy(r => r.Decision)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class TestResult
    {
        public bool Success { get; set; }
        public long ResponseTime { get; set; }
        public string Decision { get; set; } = "";
        public double Confidence { get; set; }
        public string Reasoning { get; set; } = "";
        public string Error { get; set; } = "";
    }

    public class BenchmarkResult
    {
        public TestResult HealthCheck { get; set; } = new();
        public TestResult SingleRequest { get; set; } = new();
        public List<TestResult> LoadTestResults { get; set; } = new();
        public Dictionary<string, TestResult> RequestTypeResults { get; set; } = new();
        public BenchmarkSummary Summary { get; set; } = new();
    }

    public class BenchmarkSummary
    {
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public long MinResponseTime { get; set; }
        public long MaxResponseTime { get; set; }
        public double AverageConfidence { get; set; }
        public Dictionary<string, int> DecisionDistribution { get; set; } = new();
    }

    public class ApprovalRequestDto
    {
        public string RequestType { get; set; } = "";
        public string RequesterId { get; set; } = "";
        public decimal? Amount { get; set; }
        public int? Days { get; set; }
        public string Description { get; set; } = "";
        public string Department { get; set; } = "";
        public string Priority { get; set; } = "";
    }

    public class DecisionOutputDto
    {
        public string Decision { get; set; } = "";
        public double ConfidenceScore { get; set; }
        public string Reasoning { get; set; } = "";
        public List<string> PolicyReferences { get; set; } = new();
        public string EscalationReason { get; set; } = "";
        public bool HumanReviewRequired { get; set; }
        public DateTime Timestamp { get; set; }
        public string ProcessingTime { get; set; } = "";
    }
}
