using System;
using System.Threading.Tasks;

namespace EAI.Testing
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🤖 EAI Comprehensive Testing & Benchmarking Suite");
            Console.WriteLine("=================================================");
            
            var tester = new APITester();
            
            try
            {
                // Run comprehensive benchmark
                var benchmarkResult = await tester.RunComprehensiveBenchmarkAsync();
                
                // Display results
                DisplayResults(benchmarkResult);
                
                // Generate report
                GenerateReport(benchmarkResult);
                
                Console.WriteLine("\n🎉 Benchmarking completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Benchmarking failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                tester.Dispose();
            }
        }

        static void DisplayResults(BenchmarkResult result)
        {
            Console.WriteLine("\n📊 BENCHMARK RESULTS");
            Console.WriteLine("===================");
            
            // Health Check
            Console.WriteLine($"\n🏥 Health Check:");
            Console.WriteLine($"   Status: {(result.HealthCheck.Success ? "✅ Healthy" : "❌ Failed")}");
            Console.WriteLine($"   Response Time: {result.HealthCheck.ResponseTime}ms");
            
            // Single Request
            Console.WriteLine($"\n🎯 Single Request Test:");
            Console.WriteLine($"   Status: {(result.SingleRequest.Success ? "✅ Success" : "❌ Failed")}");
            Console.WriteLine($"   Decision: {result.SingleRequest.Decision}");
            Console.WriteLine($"   Confidence: {result.SingleRequest.Confidence:F2}");
            Console.WriteLine($"   Response Time: {result.SingleRequest.ResponseTime}ms");
            Console.WriteLine($"   Reasoning: {result.SingleRequest.Reasoning}");
            
            // Load Test Summary
            Console.WriteLine($"\n⚡ Load Test Summary:");
            Console.WriteLine($"   Total Requests: {result.Summary.TotalRequests}");
            Console.WriteLine($"   Successful: {result.Summary.SuccessfulRequests}");
            Console.WriteLine($"   Failed: {result.Summary.FailedRequests}");
            Console.WriteLine($"   Success Rate: {result.Summary.SuccessRate:F1}%");
            
            // Performance Metrics
            Console.WriteLine($"\n🚀 Performance Metrics:");
            Console.WriteLine($"   Average Response Time: {result.Summary.AverageResponseTime:F0}ms");
            Console.WriteLine($"   Min Response Time: {result.Summary.MinResponseTime}ms");
            Console.WriteLine($"   Max Response Time: {result.Summary.MaxResponseTime}ms");
            Console.WriteLine($"   Average Confidence: {result.Summary.AverageConfidence:F2}");
            
            // Decision Distribution
            Console.WriteLine($"\n🎲 Decision Distribution:");
            foreach (var decision in result.Summary.DecisionDistribution)
            {
                Console.WriteLine($"   {decision.Key}: {decision.Value} requests");
            }
            
            // Request Type Results
            Console.WriteLine($"\n📋 Request Type Performance:");
            foreach (var requestType in result.RequestTypeResults)
            {
                var res = requestType.Value;
                Console.WriteLine($"   {requestType.Key.ToUpper()}:");
                Console.WriteLine($"     Status: {(res.Success ? "✅" : "❌")}");
                Console.WriteLine($"     Decision: {res.Decision}");
                Console.WriteLine($"     Confidence: {res.Confidence:F2}");
                Console.WriteLine($"     Response Time: {res.ResponseTime}ms");
            }
        }

        static void GenerateReport(BenchmarkResult result)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var reportPath = $"benchmark_report_{timestamp}.json";
            
            var report = new
            {
                Timestamp = DateTime.Now,
                Environment = new
                {
                    BaseUrl = "https://localhost:58080",
                    LLMProvider = "LM Studio",
                    Model = "qwen2.5-7b-instruct-1m",
                    Database = "In-Memory"
                },
                Results = result,
                Recommendations = GenerateRecommendations(result)
            };
            
            var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            System.IO.File.WriteAllText(reportPath, json);
            Console.WriteLine($"\n📄 Detailed report saved to: {reportPath}");
        }

        static string[] GenerateRecommendations(BenchmarkResult result)
        {
            var recommendations = new List<string>();
            
            if (result.Summary.SuccessRate < 95)
            {
                recommendations.Add("⚠️ Success rate below 95% - investigate failed requests");
            }
            
            if (result.Summary.AverageResponseTime > 2000)
            {
                recommendations.Add("🐌 Average response time > 2s - consider performance optimization");
            }
            
            if (result.Summary.MaxResponseTime > 5000)
            {
                recommendations.Add("🚨 Max response time > 5s - investigate slow requests");
            }
            
            if (result.Summary.AverageConfidence < 0.7)
            {
                recommendations.Add("🤔 Low average confidence - review LLM prompts and policies");
            }
            
            if (result.LoadTestResults.Count > 0)
            {
                var loadTestSuccessRate = result.LoadTestResults.Count(r => r.Success) / (double)result.LoadTestResults.Count * 100;
                if (loadTestSuccessRate < 90)
                {
                    recommendations.Add("⚡ Load test success rate < 90% - consider scaling improvements");
                }
            }
            
            if (recommendations.Count == 0)
            {
                recommendations.Add("✅ All metrics look good! System is performing well.");
            }
            
            return recommendations.ToArray();
        }
    }
}
