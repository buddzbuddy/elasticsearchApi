using elasticsearchApi.Data.Entities;

namespace elasticsearchApi.Services
{
    public interface INotificationService
    {
        Task SendUserCreatedNotification(User user);
    }

    public class DummyNotificationService : INotificationService
    {
        public Task SendUserCreatedNotification(User user)
        {
            Console.WriteLine($"User {user.FirstName} {user.LastName} was added!");
            return Task.CompletedTask;
        }
    }
}
