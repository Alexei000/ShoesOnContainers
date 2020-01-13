using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using ShoesOnContainers.Services.CartApi.Model;

namespace CartApi.Messaging.Consumers
{
    public class OrderCompletedEventConsumer : IConsumer<OrderCompletedEvent>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<OrderCompletedEventConsumer> _logger;

        public OrderCompletedEventConsumer(ICartRepository cartRepository, ILogger<OrderCompletedEventConsumer> logger)
        {
            this._cartRepository = cartRepository;
            _logger = logger;
        }

        public Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
            _logger.LogInformation("OrderCompletedEventConsumer.Consume started");
            return _cartRepository.DeleteCartAsync(context.Message.BuyerId);
        }
    }
}
