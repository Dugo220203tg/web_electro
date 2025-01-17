using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Interfaces;
using API_Web_Shop_Electronic_TD.Models;
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
		[HttpPost("MarkNotificationAsSeen")]
		public async Task<IActionResult> MarkNotificationAsSeen([FromBody] NotificationUpdateRequest request)
		{
			if (request?.Id == null)
				return BadRequest("Notification Id is required");

			var result = await _notificationRepository.MarkNotificationAsSeenAsync(request.Id);

			if (result)
				return Ok(new { message = "Notification marked as seen successfully" });
			else
				return NotFound(new { message = "Notification not found or already seen" });
		}
	}
}
