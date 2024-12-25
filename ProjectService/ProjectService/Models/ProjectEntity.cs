using ProjectServiceProto;

namespace ProjectService.Models;

public class ProjectEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public List<Guid> ParticipantIds { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public static class ProjectEntityExtensions
{
    public static Project ToProto(this ProjectEntity entity) =>
        new Project()
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            Description = entity.Description,
            OwnerId = entity.OwnerId.ToString(),
            ParticipantIds = { entity.ParticipantIds.Select(id => id.ToString()) },
            CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(entity.CreatedAt),
            UpdatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(entity.UpdatedAt),
        };
}
