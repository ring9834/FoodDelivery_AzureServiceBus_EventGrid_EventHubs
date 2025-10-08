using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Core.Interfaces
{
    public interface INotificationHubService
    {
        Task SendPushNotificationAsync(string userId, string title, string message, Dictionary<string, string>? data = null);
        Task SendPushNotificationToTagAsync(string tag, string title, string message, Dictionary<string, string>? data = null);
    }
}