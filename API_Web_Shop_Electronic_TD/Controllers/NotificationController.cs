using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class NotificationController : Controller
	{
		private readonly INotificationRepository _notificationRepository;

		public NotificationController(Hshop2023Context db, INotificationRepository notificationRepository)
		{
			_notificationRepository = notificationRepository;
		}
		[HttpGet]
		public async Task<IActionResult> GetUnseenNotifications()
		{
			var notifications = await _notificationRepository.GetUnseenNotificationsAsync();
			return Ok(notifications); 
		}

		[HttpGet]
		public async Task<IActionResult> GetAllNotifications()
		{
			var notifications = await _notificationRepository.GetAll();
			return Ok(notifications);
		}
		[HttpPost]
		public async Task<IActionResult> MarkNotificationsAsSeen()
		{
			bool result = await _notificationRepository.MarkNotificationsAsSeenAsync();
			if (result)
			{
				return Ok("Tất cả thông báo chưa đọc đã được đánh dấu là đã đọc.");
			}
			return NotFound("Không có thông báo nào để đánh dấu.");
		}

	}
}
