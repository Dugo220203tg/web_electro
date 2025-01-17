using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace API_Web_Shop_Electronic_TD.Repository
{
	public class NotificationRepository : INotificationRepository
	{
		private readonly Hshop2023Context _db;

		public NotificationRepository(Hshop2023Context db)
		{
			_db = db;
		}

		public async Task<List<Notification>> GetAll()
		{
			return await _db.Notifications.ToListAsync();
		}

		public async Task<List<Notification>> GetUnseenNotificationsAsync()
		{
			return await _db.Notifications
				.Where(n => (bool)!n.Status) 
				.OrderByDescending(n => n.CreateAt)
				.ToListAsync();
		}

		public async Task<bool> MarkNotificationAsSeenAsync(int notificationId)
		{
			try
			{
				var notification = await _db.Notifications
					.FirstOrDefaultAsync(n => n.Id == notificationId && (!n.Status.GetValueOrDefault()));
				if (notification == null)
					return false;

				notification.Status = true;
				await _db.SaveChangesAsync();
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		// Phương thức lấy ra trạng thái hiện tại của notification
		public async Task<bool> GetNotificationStatusAsync(int notificationId)
		{
			var notification = await _db.Notifications
				.FirstOrDefaultAsync(n => n.Id == notificationId);

			return notification?.Status ?? false;
		}
	}

}
