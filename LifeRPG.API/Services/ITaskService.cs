using LifeRPG.API.DTOs;

namespace LifeRPG.API.Services
{
    public interface ITaskService
    {
        Task<List<TaskResponseDto>> GetTasksAsync(Guid userId);
        Task<(TaskResponseDto? Result, string? ErrorMessage)> CreateTaskAsync(CreateTaskDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateTaskAsync(Guid id, CreateTaskDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteTaskAsync(Guid id);
        Task<(CompleteTaskResultDto? Result, string? ErrorMessage)> CompleteTaskAsync(Guid id);
    }
}