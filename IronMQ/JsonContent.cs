#region Apache 2 License
// Copyright (c) Applied Duality, Inc., All rights reserved.
// See License.txt in the project root for license information.
#endregion

using System.Net.Http;

// Great blog post about HttpClient
// http://pfelix.wordpress.com/2012/01/11/the-new-net-httpclient-class/

namespace System.Reactive.IronMQ
{
    /// <summary>
    /// JsonContent surpressing encoding since that trips up IronMQ.
    /// </summary>
    public class JsonContent : StringContent
    {
        public JsonContent() : this(content: "{}") { }

        public JsonContent(System.Json.JsonValue content) : this(content: content.ToString()) { }

        public JsonContent(string content)
            : base(content: content)
        {
            base.Headers.ContentType.CharSet = "";
            base.Headers.ContentType.MediaType = "application/json";
        }
    }
}
