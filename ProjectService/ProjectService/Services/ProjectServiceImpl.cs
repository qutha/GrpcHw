using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProjectService.Models;
using ProjectServiceProto;

namespace ProjectService.Services;

public class ProjectServiceImpl(ILogger<ProjectServiceImpl> logger, ProjectDbContext db)
    : ProjectServiceProto.ProjectService.ProjectServiceBase
{
    public override async Task<ProjectResponse> CreateProject(
        CreateProjectRequest request,
        ServerCallContext context
    )
    {
        _ = Guid.TryParse(request.OwnerId, out var ownerId);
        var project = new ProjectEntity
        {
            Name = request.Name,
            Description = request.Description,
            OwnerId = ownerId,
            ParticipantIds =
            [
                .. request
                    .ParticipantIds.Select(p =>
                    {
                        _ = Guid.TryParse(p, out var pId);

                        return pId;
                    })
                    .Where(p => p != default),
            ],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await db.Projects.AddAsync(project);
        await db.SaveChangesAsync();

        return new ProjectResponse { Project = project.ToProto() };
    }

    public override async Task<ProjectResponse> GetProject(
        GetProjectRequest request,
        ServerCallContext context
    )
    {
        _ = Guid.TryParse(request.Id, out var projectId);
        var project =
            await db.Projects.FindAsync(projectId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Проект не найден"));
        return new ProjectResponse { Project = project.ToProto() };
    }

    public override async Task<ProjectResponse> UpdateProject(
        UpdateProjectRequest request,
        ServerCallContext context
    )
    {
        _ = Guid.TryParse(request.Id, out var projectId);
        var participantIds = request
            .ParticipantIds.Select(p =>
            {
                _ = Guid.TryParse(p, out var pId);
                return pId;
            })
            .Where(p => p != default)
            .ToList();

        var project =
            await db.Projects.FindAsync(projectId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Проект не найден"));
        project.Name = request.Name ?? project.Name;
        project.Description = request.Description ?? project.Description;
        if (participantIds.Count != 0)
        {
            project.ParticipantIds = participantIds;
        }
        project.UpdatedAt = DateTime.UtcNow;

        db.Projects.Update(project);
        await db.SaveChangesAsync();

        return new ProjectResponse { Project = project.ToProto() };
    }

    public override async Task<Response> DeleteProject(
        DeleteProjectRequest request,
        ServerCallContext context
    )
    {
        _ = Guid.TryParse(request.Id, out var projectId);
        var project =
            await db.Projects.FindAsync(projectId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Проект не найден"));
        db.Projects.Remove(project);
        await db.SaveChangesAsync();

        return new Response { Message = "Проект успешно удален", Success = true };
    }

    public override async Task<ListProjectsResponse> ListProjects(
        ListProjectsRequest request,
        ServerCallContext context
    )
    {
        var query = db.Projects.AsQueryable();

        if (Guid.TryParse(request.OwnerId, out var ownerId))
        {
            query = query.Where(p => p.OwnerId == ownerId);
        }

        var participantIds = request
            .ParticipantIds.Select(p =>
            {
                _ = Guid.TryParse(p, out var pId);
                return pId;
            })
            .Where(p => p != default)
            .ToList();
        if (participantIds.Count != 0)
        {
            query = query.Where(p => p.ParticipantIds.Any(id => participantIds.Contains(id)));
        }

        var projects = await query.ToListAsync();

        return new ListProjectsResponse { Projects = { projects.Select(p => p.ToProto()) } };
    }

    public override async Task<Response> AddParticipant(
        AddParticipantRequest request,
        ServerCallContext context
    )
    {
        _ = Guid.TryParse(request.ProjectId, out var projectId);
        var project =
            await db.Projects.FindAsync(projectId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Проект не найден"));
        _ = Guid.TryParse(request.UserId, out var userId);

        if (!project.ParticipantIds.Contains(userId))
        {
            project.ParticipantIds.Add(userId);
            project.UpdatedAt = DateTime.UtcNow;

            db.Projects.Update(project);
            await db.SaveChangesAsync();
        }

        return new Response { Message = "Участник успешно добавлен", Success = true };
    }

    public override async Task<Response> RemoveParticipant(
        RemoveParticipantRequest request,
        ServerCallContext context
    )
    {
        _ = Guid.TryParse(request.ProjectId, out var projectId);
        var project =
            await db.Projects.FindAsync(projectId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Проект не найден"));
        var userId = Guid.Parse(request.UserId);

        var wasRemoved = project.ParticipantIds.Remove(userId);
        if (wasRemoved)
        {
            project.UpdatedAt = DateTime.UtcNow;

            db.Projects.Update(project);
            await db.SaveChangesAsync();
        }

        return new Response { Message = "Участник успешно удален", Success = true };
    }
}
