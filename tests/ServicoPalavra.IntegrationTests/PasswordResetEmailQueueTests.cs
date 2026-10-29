using Microsoft.Extensions.Logging.Abstractions;
using ServicoPalavra.Application.Auth;
using ServicoPalavra.Infrastructure.Email;

namespace ServicoPalavra.IntegrationTests;

public sealed class PasswordResetEmailQueueTests
{
    [Fact]
    public void Queue_is_bounded_and_rejects_when_capacity_is_exhausted()
    {
        var queue = new PasswordResetEmailQueue(NullLogger<PasswordResetEmailQueue>.Instance);
        var job = new PasswordResetEmailJob(
            "queue@tests.local",
            "Queue Test",
            Guid.NewGuid(),
            "token");

        for (var index = 0; index < 100; index++)
        {
            Assert.True(queue.TryEnqueue(job));
        }

        Assert.False(queue.TryEnqueue(job));
    }
}
