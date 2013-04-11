#region Apache 2 License
// Copyright (c) Applied Duality, Inc., All rights reserved.
// See License.txt in the project root for license information.
#endregion

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.IronMQ
{
    public static class Extensions
    {
        /// <summary>
        /// Hack to do ToString() on enum.
        /// </summary>
        public static string toString(this Cloud cloud)
        {
            switch (cloud)
            {
                case Cloud.AWS: return "mq-aws-us-east-1";
                case Cloud.RackSpace: return "mq-rackspace-dfw";
                default: return null;
            }
        }

        /// <summary>
        /// Send a DELETE request to the specified Uri with a cancellation token as an asynchronous operation.
        /// </summary>
        public static Task<HttpResponseMessage> DeleteAsync(this HttpClient client, string requestUri, HttpContent content, CancellationToken token)
        {
            var message = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            message.Content = content;
            return client.SendAsync(message, token);
        }

        /// <summary>
        /// Send a DELETE request to the specified Uri as an asynchronous operation.
        /// </summary>
        public static Task<HttpResponseMessage> DeleteAsync(this HttpClient client, string requestUri, HttpContent content)
        {
            return client.DeleteAsync(requestUri, content, CancellationToken.None);
        }
    }
}
