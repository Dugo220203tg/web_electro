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

		// Cập nhật tất cả các thông báo có Status = false thành Status = true
		public async Task<bool> MarkNotificationsAsSeenAsync()
		{
			var notifications = await _db.Notifications
				.Where(n => (bool)!n.Status) 
				.ToListAsync();

			if (!notifications.Any())
				return false;

			foreach (var notification in notifications)
			{
				notification.Status = true; 
			}

			await _db.SaveChangesAsync(); 
			return true; 
		}
	}

}
