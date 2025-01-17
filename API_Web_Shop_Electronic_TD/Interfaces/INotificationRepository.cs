using API_Web_Shop_Electronic_TD.Data;

namespace API_Web_Shop_Electronic_TD.Interfaces
{
	public interface INotificationRepository
	{
		Task<List<Notification>> GetUnseenNotificationsAsync();
		Task<bool> MarkNotificationAsSeenAsync(int notificationId);
		Task<List<Notification>> GetAll();
		Task<bool> GetNotificationStatusAsync(int notificationId);
	}
}
