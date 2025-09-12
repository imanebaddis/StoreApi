using Microsoft.AspNetCore.Mvc;
using StoreApi.Infra.DTOs;
using StoreApi.Infra.Services;
using System.Collections.Generic;

namespace StoreApi.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService) {
            _subscriptionService = subscriptionService;
        }

        [HttpGet("plans")]
        public ActionResult<IEnumerable<SubscriptionPlanDto>> GetAvailablePlans() {
            var plans = _subscriptionService.GetAvailablePlans();
            return Ok(plans);
        }

        [HttpPost]
        public ActionResult<SubscriptionResponseDto> CreateSubscription(CreateSubscriptionDto dto) {
            try {
                var subscription = _subscriptionService.CreateSubscription(dto);
                return CreatedAtAction(nameof(GetActiveSubscription), new { userId = dto.UserId }, subscription);
            }
            catch (ArgumentException ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user/{userId}")]
        public ActionResult<SubscriptionResponseDto> GetActiveSubscription(int userId) {
            var subscription = _subscriptionService.GetActiveSubscription(userId);
            return subscription == null ? NotFound() : Ok(subscription);
        }

        [HttpGet("user/{userId}/history")]
        public ActionResult<IEnumerable<SubscriptionResponseDto>> GetUserSubscriptions(int userId) {
            var subscriptions = _subscriptionService.GetUserSubscriptions(userId);
            return Ok(subscriptions);
        }

        [HttpPatch("{id}/cancel")]
        public IActionResult CancelSubscription(int id) {
            _subscriptionService.CancelSubscription(id);
            return NoContent();
        }

        [HttpPatch("{id}/auto-renew")]
        public IActionResult AutoRenewSubscription(int id) {
            _subscriptionService.AutoRenewSubscription(id);
            return NoContent();
        }

        [HttpGet("user/{userId}/can-access/{bookId}")]
        public ActionResult<bool> CanAccessBook(int userId, int bookId) {
            var canAccess = _subscriptionService.CanAccessBook(userId, bookId);
            return Ok(canAccess);
        }
    }
}