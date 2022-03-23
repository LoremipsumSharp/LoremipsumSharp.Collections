using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace LoremipsumSharp.RabbitMq
{
    public interface IChannelFactory : IDisposable
    {

        /// <summary>
        ///  create the channel
        /// </summary>
        /// <param name="token"></param>
        /// <param name="key"></param>
        /// <returns></returns>
		Task<IModel> CreateChannelAsync(string key, CancellationToken token = default(CancellationToken));
    }


    public class ChannelFactory : IChannelFactory
    {
        private readonly ILogger _logger;
        protected readonly Func<string, IConnectionFactory> ConnectionFactorySelector;
        protected readonly ConcurrentBag<IModel> Channels;
        protected ConcurrentDictionary<string, IConnection> ConnectionDict;

        public ChannelFactory(Func<string, IConnectionFactory> connectionFactorySelector, ILogger logger)
        {

            ConnectionFactorySelector = connectionFactorySelector;
            Channels = new ConcurrentBag<IModel>();
            ConnectionDict = new ConcurrentDictionary<string, IConnection>();
            _logger = logger;
        }

        public virtual Task<IConnection> ConnectAsync(string key, CancellationToken token = default(CancellationToken))
        {
            try
            {
                _logger.LogDebug($"Creating a new connection for {key}");
                var connection = ConnectionDict.GetOrAdd(key, (key) =>
                {
                    return ConnectionFactorySelector(key).CreateConnection();
                });
                connection.ConnectionShutdown += (sender, args) =>
                    _logger.LogWarning($"Connection was shutdown by {args.Initiator}. ReplyText {args.ReplyText}");
                return Task.FromResult(connection);
            }
            catch (BrokerUnreachableException e)
            {
                _logger.LogError("Unable to connect to broker", e);
                throw;
            }

        }

        public virtual async Task<IModel> CreateChannelAsync(string key, CancellationToken token = default(CancellationToken))
        {
            var connection = await GetConnectionAsync(key, token);
            token.ThrowIfCancellationRequested();
            var channel = connection.CreateModel();
            Channels.Add(channel);
            return channel;
        }

        protected virtual async Task<IConnection> GetConnectionAsync(string key, CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();
            IConnection connection = await ConnectAsync(key, token);
            if (connection.IsOpen)
            {
                _logger.LogDebug("Existing connection is open and will be used.");
                return connection;
            }
            _logger.LogInformation("The existing connection is not open.");

            if (connection.CloseReason != null && connection.CloseReason.Initiator == ShutdownInitiator.Application)
            {
                _logger.LogInformation("Connection is closed with Application as initiator. It will not be recovered.");
                connection.Dispose();
                throw new InvalidOperationException("Closed connection initiated by the Application. A new connection will not be created, and no channel can be created.");
            }

            if (!(ConnectionDict is IRecoverable recoverable))
            {
                _logger.LogInformation("Connection is not recoverable");
                connection.Dispose();
                throw new InvalidOperationException("The non recoverable connection is closed. A channel can not be created.");
            }

            _logger.LogDebug("Connection is recoverable. Waiting for 'Recovery' event to be triggered. ");
            var recoverTcs = new TaskCompletionSource<IConnection>();
            token.Register(() => recoverTcs.TrySetCanceled());

            EventHandler<EventArgs> completeTask = null;
            completeTask = (sender, args) =>
            {
                if (recoverTcs.Task.IsCanceled)
                {
                    return;
                }
                _logger.LogInformation("Connection has been recovered!");
                recoverTcs.TrySetResult(recoverable as IConnection);
                recoverable.Recovery -= completeTask;
            };

            recoverable.Recovery += completeTask;
            return await recoverTcs.Task;
        }

        public void Dispose()
        {
            foreach (var channel in Channels)
            {
                channel?.Dispose();
            }
            foreach(var kv in this.ConnectionDict)
            {
                kv.Value?.Dispose();
            }
        }
    }
}