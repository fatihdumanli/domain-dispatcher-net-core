using System;
using Autofac;
using DomainDispatching.Commanding;
using DomainDispatching.DomainEvent;
using Microsoft.Extensions.Logging;

namespace DomainDispatching
{
    public class DomainDispatcher
    {   
        private ILogger<DomainDispatcher> _logger;
        private ILifetimeScope _scope;
        public DomainDispatcher(ILifetimeScope scope, ILogger<DomainDispatcher> logger = null)
        {
           _logger = logger;
           _scope = scope;
           
           _logger.LogInformation(" [x] DomainDispatcher: Creating an instance of DomainDispatcher.");
        }

        public void DispatchCommand(Command command)
        {
            using(var currentScope = _scope.BeginLifetimeScope())
            {
                try
                {
                    _logger.LogInformation(string.Format(" [x] DomainDispatcher.DispatchCommand(): Dispatching the command {0}", command.GetType().Name));
                    var commandHandlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
                    var commandHandler = currentScope.Resolve(commandHandlerType);
                    _logger.LogInformation(string.Format(" [x] DomainDispatcher.DispatchCommand(): Resolved an instance of {0}", commandHandler.GetType().Name));
                    commandHandlerType.GetMethod("Handle").Invoke(commandHandler, new object[] { command });
                } 
                
                catch(Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
               
            }
        }
    
        public void PublishDomainEvent(IDomainEvent @event)
        {
            using(var currentScope = _scope.BeginLifetimeScope())
            {
                try
                {
                    _logger.LogInformation(string.Format(" [x] DomainDispatcher.PublishDomainEvent(): Publishing the domain event: {0}", @event.GetType().Name));
                    var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(@event.GetType());
                    var domainEventHandler = currentScope.Resolve(typeof(IDomainEventHandler<>).MakeGenericType(@event.GetType()));
                    _logger.LogInformation(string.Format(" [x] DomainDispatcher.PublishDomainEvent(): Resolved an instance of {0}", domainEventHandler.GetType().Name));
                    handlerType.GetMethod("Handle").Invoke(domainEventHandler, new object[] { @event });
                }

                catch(Exception ex)
                {
                    _logger.LogError(ex.Message);
                }           
            }
        }
    }
}