# Enterprise Approval Intelligence (EAI)

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/License-Apache%202.0-green.svg)](https://opensource.org/licenses/Apache-2.0)
[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen.svg)](https://github.com/your-org/eai)
[![LLM](https://img.shields.io/badge/LLM-qwen2.5--7b--instruct--1m-orange.svg)](https://lmstudio.ai)
[![Docker](https://img.shields.io/badge/Docker-Supported-blue.svg)](https://www.docker.com/)
[![Tests](https://img.shields.io/badge/Tests-Passing-green.svg)](https://github.com/your-org/eai)

A C# .NET-based enterprise framework for transforming approvals from traditional status-based workflows to intelligent, autonomous decision-making systems powered by local LLM integration (LM Studio/qwen2.5-7b-instruct-1m) with enterprise-grade security, performance, and compliance.

## 🚀 Features

- **🤖 AI-Powered Decisions**: Local LLM integration with LM Studio and qwen2.5-7b-instruct-1m model
- **🧠 Autonomous Decision Making**: AI-powered approval decisions with confidence scoring
- **📋 Policy Engine**: Dynamic policy interpretation and conflict resolution
- **📊 Audit System**: Comprehensive audit trails and compliance reporting
- **🔒 Enterprise Security**: Encryption, authentication, and role-based access control
- **⚡ High Performance**: Async/await patterns and in-memory database for fast testing
- **🐳 Docker Support**: Containerized deployment with Docker and Kubernetes
- **🧪 Comprehensive Testing**: Load testing, benchmarking, and JSON report generation

## 🏗️ Architecture

The EAI system follows a clean architecture pattern with the following layers:

- **EAI.Core**: Business logic, interfaces, and domain models
- **EAI.Api**: REST API controllers and middleware
- **EAI.Infrastructure**: Data access, external services, and LLM integrations
- **EAI.Tests**: Unit and integration tests
- **EAI.Testing**: Comprehensive load testing and benchmarking suite

## 📋 Prerequisites

- .NET 8.0 SDK
- **LM Studio** with **qwen2.5-7b-instruct-1m** model (for AI decisions)
- In-memory database (no SQL Server required for testing)
- Docker Desktop (optional)
- Visual Studio 2022 or VS Code

## 🚀 Quick Start

### 1. Clone and Setup

```bash
git clone https://github.com/your-org/eai.git
cd eai
```

### 2. Setup LM Studio

1. **Install LM Studio**: Download from [lmstudio.ai](https://lmstudio.ai)
2. **Download qwen2.5-7b-instruct-1m**: Pull the model in LM Studio
3. **Start LM Studio Server**: Run on `http://localhost:1234`

### 3. Build and Start the Application

```bash
# Build the solution
dotnet build EAI.sln

# Start the API
dotnet run --project src/EAI.Api --urls "https://localhost:58080"
```

### 4. Access the API

- **API**: https://localhost:58080
- **Health Check**: https://localhost:58080/health
- **Approval Endpoint**: https://localhost:58080/api/approval/process

## 📖 Usage Examples

### Process an Approval Request

```csharp
var request = new ApprovalRequest
{
    RequestType = "expense",
    RequesterId = "user123",
    Amount = 500.00m,
    Description = "Business travel expenses",
    Department = "Sales",
    Project = "Client Meeting",
    Priority = "normal"
};

var decision = await orchestrator.ProcessApprovalRequestAsync(request);

Console.WriteLine($"Decision: {decision.Decision}");
Console.WriteLine($"Confidence: {decision.ConfidenceScore:P}");
Console.WriteLine($"Reasoning: {decision.Reasoning}");
```

### API Endpoints

#### Process Approval
```http
POST /api/approval/process
Content-Type: application/json

{
  "requestType": "expense",
  "requesterId": "user123",
  "amount": 500,
  "description": "Business travel expenses",
  "department": "Sales",
  "priority": "normal"
}
```

#### Get Audit Trail
```http
GET /api/audit/report?startDate=2024-01-01&endDate=2024-12-31
```

#### List Policies
```http
GET /api/policy
```

## 🧪 Testing & Benchmarking

### Prerequisites for Testing
1. **Start the API first**:
   ```bash
   dotnet run --project src/EAI.Api --urls "https://localhost:58080"
   ```

2. **Ensure LM Studio is running** on `http://localhost:1234`

### Run Comprehensive Tests
```bash
# Run the complete testing suite
dotnet run --project src/EAI.Testing
```

### Test Output
The testing suite generates detailed JSON reports:
- **File**: `benchmark_report_YYYY-MM-DD_HH-mm-ss.json`
- **Includes**: Performance metrics, decision accuracy, LLM integration results
- **Metrics**: Response times, success rates, confidence scores, decision distribution

### Test Coverage
- ✅ **Health Check**: API availability
- ✅ **Single Request**: Individual approval processing
- ✅ **Load Testing**: 5 concurrent users, 10 requests each
- ✅ **Request Types**: Expense, Leave, Purchase approvals
- ✅ **LLM Integration**: Real AI decision making with qwen2.5-7b-instruct-1m
- ✅ **Performance Metrics**: Response times, throughput, accuracy

## 🐳 Docker Deployment

### Build and Run with Docker Compose
```bash
docker-compose up --build
```

### Kubernetes Deployment
```bash
kubectl apply -f k8s/deployment.yaml
```

## 📊 Monitoring & Observability

- **Structured Logging**: JSON logs with Serilog
- **Health Checks**: Database, Redis, and external service monitoring
- **Metrics**: Decision accuracy, processing latency, throughput
- **Audit Trails**: Complete decision history with encryption

## 🔒 Security Features

- **Data Encryption**: AES-256 encryption for sensitive data
- **Authentication**: JWT-based authentication
- **Authorization**: Role-based access control
- **Audit Logging**: Comprehensive security event logging
- **Input Validation**: Request validation and sanitization

## 📈 Performance

- **Async Operations**: Non-blocking I/O operations
- **Caching**: Redis-based caching for policies and decisions
- **Connection Pooling**: Optimized database connections
- **Rate Limiting**: API rate limiting and throttling

## 🔧 Configuration

### LLM Configuration (appsettings.json)
```json
{
  "LLM": {
    "Provider": "LMStudio",
    "Model": "qwen2.5-7b-instruct-1m",
    "LMStudioUrl": "http://localhost:1234",
    "MaxTokens": 1000,
    "Temperature": 0.7,
    "TopP": 0.9
  },
  "Database": {
    "UseInMemory": true,
    "SeedData": true
  }
}
```

### Environment Variables
- `LLM__LMStudioUrl`: LM Studio server URL (default: http://localhost:1234)
- `LLM__Model`: Model name (default: qwen2.5-7b-instruct-1m)
- `Database__UseInMemory`: Use in-memory database (default: true)

### Policy Configuration
Policies are defined in JSON format in the `config/policies/` directory:

```json
{
  "policyId": "expense_policy_v1",
  "policyName": "Expense Approval Policy",
  "rules": [
    {
      "condition": "amount <= 100",
      "action": "auto_approve",
      "confidenceThreshold": 0.9
    }
  ]
}
```
## 📄 License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author & Credits

**Enterprise Approval Intelligence (EAI)**  
Developed by: **Mateus Yonathan**

### Attribution Notice
When using this software, please include the following attribution:

```
Enterprise Approval Intelligence (EAI)
Copyright (c) 2025 Mateus Yonathan
Licensed under the Apache License, Version 2.0
```

---

**Enterprise Approval Intelligence** - Transforming enterprise workflows with AI-powered autonomous decision making using local LLM integration.
