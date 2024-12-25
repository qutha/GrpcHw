using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using TaskService.Models;
using TaskServiceProto;

namespace TaskService.Services;

public class TaskServiceImpl(ILogger<TaskServiceImpl> logger, TaskDbContext db)
    : TaskServiceProto.TaskService.TaskServiceBase
{
    public override async Task<TaskResponse> CreateTask(
        CreateTaskRequest request,
        ServerCallContext context
    )
    {
        _ = Guid.TryParse(request.ProjectId, out Guid projectId);
        _ = Guid.TryParse(request.AssigneeId, out Guid assigneeId);

        var task = new TaskEntity()
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = projectId,
            AssigneeId = assigneeId,
            Tags = [.. request.Tags],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _ = await db.Tasks.AddAsync(task);
        _ = await db.SaveChangesAsync();

        return new TaskResponse() { Task = task.ToProto() };
    }

    public override async Task<TaskResponse> GetTask(
        GetTaskRequest request,
        ServerCallContext context
    )
    {
        _ = Guid.TryParse(request.Id, out Guid taskId);

        var task =
            await db.Tasks.FindAsync(taskId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Задача не найдена"));
        return new TaskResponse { Task = task.ToProto() };
    }

    public override async Task<TaskResponse> UpdateTask(
        UpdateTaskRequest request,
        ServerCallContext context
    )
    {
        _ = Guid.TryParse(request.Id, out Guid taskId);
        var isAssigneeIdCorrect = Guid.TryParse(request.Id, out Guid assigneeId);
        var task =
            await db.Tasks.FindAsync(taskId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Задача не найдена"));

        task.Title = request.Title ?? task.Title;
        task.Description = request.Description ?? task.Description;
        task.AssigneeId = isAssigneeIdCorrect ? assigneeId : task.AssigneeId;
        task.Tags = request.Tags.Count != 0 ? [.. request.Tags] : task.Tags;
        task.UpdatedAt = DateTime.UtcNow;

        _ = db.Tasks.Update(task);
        _ = await db.SaveChangesAsync();

        return new TaskResponse { Task = task.ToProto() };
    }

    public override async Task<Response> DeleteTask(
        DeleteTaskRequest request,
        ServerCallContext context
    )
    {
        _ = Guid.TryParse(request.Id, out Guid taskId);
        var task =
            await db.Tasks.FindAsync(taskId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Задача не найдена"));

        _ = db.Tasks.Remove(task);
        _ = await db.SaveChangesAsync();

        return new Response { Message = "Задача успешно удалена", Success = true };
    }

    public override async Task<ListTasksResponse> ListTasks(
        ListTasksRequest request,
        ServerCallContext context
    )
    {
        IQueryable<TaskEntity> query = db.Tasks.AsQueryable();

        if (Guid.TryParse(request.ProjectId, out var projectId))
        {
            query = query.Where(t => t.ProjectId == projectId);
        }

        if (Guid.TryParse(request.ProjectId, out var assigneeId))
        {
            query = query.Where(t => t.AssigneeId == assigneeId);
        }

        if (request.Tags.Count != 0)
        {
            query = query.Where(t => t.Tags.Any(tag => request.Tags.Contains(tag)));
        }

        List<TaskEntity> tasks = await query.ToListAsync();

        return new ListTasksResponse { Tasks = { tasks.Select(t => t.ToProto()) } };
    }
}
