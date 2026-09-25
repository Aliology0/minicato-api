using AlMostashar.Application.Common.Interfaces;

namespace AlMostashar.Infrastructure.Services;

internal sealed class DevelopmentNoOpFcmService : IFcmService
{
    public Task SendToTokenAsync(
        string token,
        Dictionary<string, string> data,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SendToMultipleTokensAsync(
        IEnumerable<string> tokens,
        Dictionary<string, string> data,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SendToTopicAsync(
        string topic,
        Dictionary<string, string> data,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SendToUserByIdAsync(
        int userId,
        Dictionary<string, string> data,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SendCallNotificationAsync(
        int userId,
        Dictionary<string, string> data,
        CancellationToken cancellationToken = default) => Task.CompletedTask;
}