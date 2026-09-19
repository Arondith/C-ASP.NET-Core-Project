using TaskForge.Api.Domain;
using TaskForge.Api.Services;
using Xunit;

namespace TaskForge.Api.Tests;

public sealed class WorkItemRulesTests
{
    [Theory]
    [InlineData(WorkItemStatus.Todo, WorkItemStatus.InProgress)]
    [InlineData(WorkItemStatus.Todo, WorkItemStatus.Cancelled)]
    [InlineData(WorkItemStatus.InProgress, WorkItemStatus.Blocked)]
    [InlineData(WorkItemStatus.InProgress, WorkItemStatus.Done)]
    [InlineData(WorkItemStatus.Blocked, WorkItemStatus.InProgress)]
    public void CanTransition_ReturnsTrue_ForAllowedTransitions(
        WorkItemStatus from,
        WorkItemStatus to)
    {
        Assert.True(WorkItemRules.CanTransition(from, to));
    }

    [Theory]
    [InlineData(WorkItemStatus.Todo, WorkItemStatus.Done)]
    [InlineData(WorkItemStatus.Blocked, WorkItemStatus.Done)]
    [InlineData(WorkItemStatus.Done, WorkItemStatus.InProgress)]
    [InlineData(WorkItemStatus.Cancelled, WorkItemStatus.Todo)]
    public void CanTransition_ReturnsFalse_ForInvalidTransitions(
        WorkItemStatus from,
        WorkItemStatus to)
    {
        Assert.False(WorkItemRules.CanTransition(from, to));
    }

    [Fact]
    public void CanTransition_AllowsIdempotentUpdate()
    {
        Assert.True(WorkItemRules.CanTransition(
            WorkItemStatus.InProgress,
            WorkItemStatus.InProgress));
    }
}
