using System.Threading.Tasks;

namespace SecuIntegrator26.Core.Interfaces
{
    public interface INotificationService
    {
        Task SendMessageAsync(string message);
    }
}
