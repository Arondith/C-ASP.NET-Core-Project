using TaskForge.Api.Domain;

namespace TaskForge.Api.Services;

public static class WorkItemRules
{
    public static bool CanTransition(WorkItemStatus from, WorkItemStatus to)
    {
        if (from == to)
        {
            return true;
        }

        return from switch
        {
            WorkItemStatus.Todo => to is WorkItemStatus.InProgress or WorkItemStatus.Cancelled,
            WorkItemStatus.InProgress => to is WorkItemStatus.Blocked or WorkItemStatus.Done or WorkItemStatus.Cancelled,
            WorkItemStatus.Blocked => to is WorkItemStatus.InProgress or WorkItemStatus.Cancelled,
            WorkItemStatus.Done => false,
            WorkItemStatus.Cancelled => false,
            _ => false
        };
    }
}
