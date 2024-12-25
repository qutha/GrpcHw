using TaskServiceProto;

namespace TaskService.Models;

public class TaskEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }
    public Guid? AssigneeId { get; set; }

    public List<string> Tags { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public static class TaskEntityExtensions
{
    public static TaskModel ToProto(this TaskEntity entity) =>
        new TaskModel()
        {
            Id = entity.Id.ToString(),
            Title = entity.Title,
            Description = entity.Description,
            ProjectId = entity.ProjectId.ToString(),
            AssigneeId = entity.AssigneeId.ToString() ?? string.Empty,
            CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(entity.CreatedAt),
            UpdatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(entity.UpdatedAt),
            Tags = { entity.Tags },
        };
}
