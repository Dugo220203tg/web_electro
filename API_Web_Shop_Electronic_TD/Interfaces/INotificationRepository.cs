using API_Web_Shop_Electronic_TD.Data;

namespace API_Web_Shop_Electronic_TD.Interfaces
{
	public interface INotificationRepository
	{
		Task<List<Notification>> GetUnseenNotificationsAsync();
		Task<bool> MarkNotificationsAsSeenAsync();
		Task<List<Notification>> GetAll();
	}
}
