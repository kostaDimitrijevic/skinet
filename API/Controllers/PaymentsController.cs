using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Errors;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Order = Core.Entities.OrderAggregate.Order;

namespace API.Controllers
{
    public class PaymentsController : BaseApiController
    {
        private readonly IPaymentService _paymentService;
        private const string WhSecret = "whsec_87bd3c7cd4708dbf8495369978acb96661afb1a5d7f680d800740644839b25cd";
        private readonly ILogger<PaymentsController> _logger;
        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        [Authorize] // ovde radimo autorizaciju a ne nad celim kontroler jer ne zelimo da stripe mora da se uloguje da bi nam poslao nesto
        [HttpPost("{basketId}")]
        public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);

            if(basket == null) return BadRequest(new ApiResponse(400, "Problem with your basket"));

            return basket;
        }

        [HttpPost("webhook")]
        public async Task<ActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], WhSecret);

            PaymentIntent intent;
            Order order;

            switch(stripeEvent.Type)
            {
                case "payment_intent.succeeded": 
                        intent = (PaymentIntent) stripeEvent.Data.Object;
                        _logger.LogInformation("Payment Succeeded", intent.Id);
                        order = await _paymentService.UpdateOrderPaymentSucceeded(intent.Id);
                        _logger.LogInformation("Order updated to payment received: ", order.Id);
                        break;
                case "payment_intent.payment_failed":
                        intent = (PaymentIntent)stripeEvent.Data.Object;
                        _logger.LogInformation("Payment Failed: ", intent.Id);
                        order = await _paymentService.UpdateOrderPaymentFailed(intent.Id);
                        _logger.LogInformation("Payment Failed: ", order.Id);
                        break;
            }

            return new EmptyResult();
        }
    }
}