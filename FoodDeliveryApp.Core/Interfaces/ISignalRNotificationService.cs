using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Core.Interfaces
{
    public interface ISignalRNotificationService
    {
        Task SendToUserAsync(string userId, string method, object data);
        Task SendToGroupAsync(string groupName, string method, object data);
        Task SendToAllAsync(string method, object data);
        Task AddToGroupAsync(string connectionId, string groupName);
        Task RemoveFromGroupAsync(string connectionId, string groupName);
    }
}