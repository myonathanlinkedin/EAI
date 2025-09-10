using EAI.Core.Models;
using EAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EAI.Infrastructure.Data;

public class DatabaseSeeder
{
    public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EAIDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();

        try
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await context.Policies.AnyAsync())
            {
                logger.LogInformation("Database already seeded");
                return;
            }

            // Seed policies
            var policies = GetSamplePolicies();
            await context.Policies.AddRangeAsync(policies);
            await context.SaveChangesAsync();

            logger.LogInformation("Database seeded with {PolicyCount} policies", policies.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding database");
            throw;
        }
    }

    private static List<PolicyDocument> GetSamplePolicies()
    {
        return new List<PolicyDocument>
        {
            new PolicyDocument
            {
                Id = "expense_policy_v1",
                PolicyName = "Expense Approval Policy",
                PolicyType = "expense",
                Content = "This policy governs expense approval processes. Expenses under $100 are auto-approved. Expenses between $100-$1000 require manager approval. Expenses over $1000 require executive approval.",
                Version = "1.0",
                EffectiveDate = DateTime.UtcNow.AddDays(-30),
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                Department = "Finance",
                IsActive = true,
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        Id = "expense_rule_1",
                        Condition = "amount <= 100",
                        Action = "auto_approve",
                        ConfidenceThreshold = 0.9,
                        Priority = "high"
                    },
                    new PolicyRule
                    {
                        Id = "expense_rule_2",
                        Condition = "100 < amount <= 1000",
                        Action = "manager_approval",
                        ConfidenceThreshold = 0.8,
                        Priority = "normal"
                    },
                    new PolicyRule
                    {
                        Id = "expense_rule_3",
                        Condition = "amount > 1000",
                        Action = "executive_approval",
                        ConfidenceThreshold = 0.7,
                        Priority = "normal"
                    }
                }
            },
            new PolicyDocument
            {
                Id = "leave_policy_v1",
                PolicyName = "Leave Approval Policy",
                PolicyType = "leave",
                Content = "This policy governs leave approval processes. Leave requests up to 5 days are auto-approved. Leave requests between 5-15 days require manager approval. Leave requests over 15 days require executive approval.",
                Version = "1.0",
                EffectiveDate = DateTime.UtcNow.AddDays(-30),
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                Department = "HR",
                IsActive = true,
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        Id = "leave_rule_1",
                        Condition = "days <= 5",
                        Action = "auto_approve",
                        ConfidenceThreshold = 0.9,
                        Priority = "high"
                    },
                    new PolicyRule
                    {
                        Id = "leave_rule_2",
                        Condition = "5 < days <= 15",
                        Action = "manager_approval",
                        ConfidenceThreshold = 0.8,
                        Priority = "normal"
                    },
                    new PolicyRule
                    {
                        Id = "leave_rule_3",
                        Condition = "days > 15",
                        Action = "executive_approval",
                        ConfidenceThreshold = 0.7,
                        Priority = "normal"
                    }
                }
            },
            new PolicyDocument
            {
                Id = "purchase_policy_v1",
                PolicyName = "Purchase Approval Policy",
                PolicyType = "purchase",
                Content = "This policy governs purchase approval processes. Purchases under $500 are auto-approved. Purchases between $500-$5000 require manager approval. Purchases over $5000 require executive approval.",
                Version = "1.0",
                EffectiveDate = DateTime.UtcNow.AddDays(-30),
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                Department = "Procurement",
                IsActive = true,
                Rules = new List<PolicyRule>
                {
                    new PolicyRule
                    {
                        Id = "purchase_rule_1",
                        Condition = "amount <= 500",
                        Action = "auto_approve",
                        ConfidenceThreshold = 0.9,
                        Priority = "high"
                    },
                    new PolicyRule
                    {
                        Id = "purchase_rule_2",
                        Condition = "500 < amount <= 5000",
                        Action = "manager_approval",
                        ConfidenceThreshold = 0.8,
                        Priority = "normal"
                    },
                    new PolicyRule
                    {
                        Id = "purchase_rule_3",
                        Condition = "amount > 5000",
                        Action = "executive_approval",
                        ConfidenceThreshold = 0.7,
                        Priority = "normal"
                    }
                }
            }
        };
    }
}
