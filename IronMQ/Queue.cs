#region Apache 2 License
// Copyright (c) Applied Duality, Inc., All rights reserved.
// See License.txt in the project root for license information.
#endregion

using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Threading.Tasks;

namespace System.Reactive.IronMQ
{
    /// <summary>
    /// All creation methods ensure that a queue with Name exist on the server-side.
    /// However, the queue can be deleted using DeleteAsync at any time, 
    /// so we cannot maintain the invariant that 
    /// a Queue instance on the client always corresponds to a queue instance on the server.
    /// </summary>
    public struct Queue
    {
        Client _client;

        internal Queue(Client client, string name):this()
        {
            _client = client;
            Name = name;
        }

        public string Name { get; private set; }

        /// <summary>
        /// Get number of messages in queue.
        /// </summary>
        public async Task<long> GetCountAsync()
        {
            var response = await _client.GetAsync(string.Format("queues/{0}", Name));
            var json = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode ? new _Size(json).Size : 0;
        }

        /// <summary>
        /// Check if queue exists.
        /// </summary>
        public async Task<bool> GetExistsAsync()
        {
            var response = await _client.GetAsync(string.Format("queues/{0}", Name));
            var json = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Delete the queue and all its messages.
        /// </summary>
        public async Task<bool> DeleteAsync()
        {
            var response = await _client.DeleteAsync(string.Format("queues/{0}", Name));
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Update queue.
        /// </summary>
        public async Task<bool> UpdateAsync(PushType pushtype = PushType.Multicast, int retries = 3, int retriesDelay = 60, params string[] subscribers)
        {
            var content = new JsonContent(content: new _Update( pushtype, retries, retriesDelay, subscribers));
            var response = await _client.PostAsync(string.Format("queues/{0}", Name), content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Add subscribers to queue.
        /// </summary>
        public async Task<bool> AddSubscribersAsync(params string[] subscribers)
        {
            var add = new _Subscribers(subscribers);
            var content = new JsonContent(content: add);
            var response = await _client.PostAsync(string.Format("queues/{0}/subscribers", Name), content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Remove subscribers from a queue.
        /// </summary>
        public async Task<bool> RemoveSubscribersAsync(params string[] subscribers)
        {
            var add = new _Subscribers(subscribers);
            var content = new JsonContent(content: add);
            var response = await _client.DeleteAsync(string.Format("queues/{0}/subscribers", Name), content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Clear all messages in a queue.
        /// </summary>
        public async Task<bool> ClearMessagesAsync()
        {
            var content = new JsonContent();
            var response = await _client.PostAsync(string.Format("queues/{0}/clear", Name), content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Add single message to a queue, 
        /// and if succeeded return message with updated ID.
        /// </summary>
        public async Task<Message?> AddMessageAsync(string body, long timeout = 60, long delay = 0, long expires = 604800)
        {
            var messages = await AddMessagesAsync(new Message(body, timeout, delay, expires));
            return (messages.Length != 0) ? messages[0] : default(Message);
        }

        /// <summary>
        /// Add messages to a queue, 
        /// and return messages added to the queue with updated ID.
        /// </summary>
        public async Task<Message[]> AddMessagesAsync(params Message[] messages)
        {
            var content = new JsonContent(new _Messages(messages));
            var response = await _client.PostAsync(string.Format("queues/{0}/messages", Name), content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return messages.Zip(new _IDs(json).IDs, (message, id) => { message.ID = id; return message; }).ToArray();
            }
            else
            {
                return new Message[] { };
            }
        }

        /// <summary>
        /// Get at most 1 messages from the queue.
        /// </summary>
        public async Task<Message?> GetMessageAsync(long timeout = 60)
        {
            var messages = await GetMessagesAsync(1, timeout);
            return (messages.Length != 0) ? messages[0] : default(Message);
        }

        /// <summary>
        /// Get at most n messages from the queue.
        /// </summary>
        public async Task<Message[]> GetMessagesAsync(int n = 1, long timeout = 60)
        {
            var response = await _client.GetAsync(string.Format("queues/{0}/messages?n={1}&timeout={2}", Name, n, timeout));
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new _Messages(content).Messages;
            }
            else
            {
                return new Message[] { };
            }
        }

        /// <summary>
        /// Peek at first n messages in the queue.
        /// </summary>
        public async Task<Message[]> PeekMessagesAsync(int n = 1)
        {
            var response = await _client.GetAsync(string.Format("queues/{0}/messages/peek?n={1}", Name, n));
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new _Messages(content).Messages;
            }
            else
            {
                return new Message[] { };
            }
        }

        /// <summary>
        /// Put message back on the queue.
        /// </summary>
        public async Task<bool> ReleaseMessageAsync(Message message, long delay = 60)
        {
            var content = new JsonContent(new _Delay(delay));
            var response = await _client.PostAsync(string.Format("queues/{0}/messages/{1}/release", Name, message.ID), content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Touching a reserved message extends its timeout by the duration specified when the message was created, 
        /// which is 60 seconds by default.
        /// </summary>
        public async Task<bool> TouchMessageAsync(Message message, long delay = 60)
        {
            var content = new JsonContent();
            var response = await _client.PostAsync(string.Format("queues/{0}/messages/{1}/touch", Name, message.ID), content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Delete message from the queue. 
        /// Be sure you call this after you’re done with a message otherwise it will be placed back on the queue.
        /// </summary>
        public async Task<bool> DeleteMessageAsync(Message message)
        {
            var response = await _client.DeleteAsync(string.Format("queues/{0}/messages/{1}", Name, message.ID));
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Status of push notifications.
        /// </summary>
        public async Task<PushInfo[]> GetPushStatusAsync(Message message)
        {
            var response = await _client.GetAsync(string.Format("queues/{0}/messages/{1}/subscribers", Name, message.ID));
            var content = await response.Content.ReadAsStringAsync();
            var json = ((JsonObject.Parse(content)["subscribers"] as JsonArray) as IEnumerable<JsonValue>);
            return json.Select(info => new PushInfo(info)).ToArray();
        }
    }
}