#region Apache 2 License
// Copyright (c) Applied Duality, Inc., All rights reserved.
// See License.txt in the project root for license information.
#endregion

using System.Collections.Generic;
using System.Json;
using System.Linq;

namespace System.Reactive.IronMQ
{
    public enum Cloud { AWS, RackSpace }

    public enum PushType { Unicast, Multicast }

    struct _Delay
    {
        dynamic _json;
        internal _Delay(long delay = 60)
        {
            _json = new JsonObject();
            if (delay != 60) _json.delay = delay;
        }

        public static implicit operator JsonValue(_Delay delay)
        {
            return delay._json;
        }

        public override string ToString()
        {
            return _json.ToString();
        }
    }

    struct _Update
    {
        dynamic _json;
        internal _Update(PushType pushtype = PushType.Multicast, int retries = 3, int retriesDelay = 60, params string[] subscribers):this()
        {
            this._json = new JsonObject();
            if (retriesDelay != 60) _json.retries_delay = retriesDelay;
            if (retries != 3) _json.retries = retries;
            if (pushtype != PushType.Multicast) _json.push_type = pushtype;
            if (subscribers.Length != 0)
            {
                _json.subscribers = new JsonArray(subscribers.Select(url => (JsonValue)new _Url(url)));
            }
        }

        public static implicit operator JsonValue(_Update update)
        {
            return update._json;
        }

        public override string ToString()
        {
            return _json.ToString();
        }
    }

    public struct _Subscribers
    {
        dynamic _json ;
        public _Subscribers(params string[] subscribers)
        {
            _json = new JsonObject();
            if (subscribers.Length != 0)
            {
                var urls = subscribers.Select(url => (JsonValue)new _Url(url));
                _json.subscribers = new JsonArray(urls);
            }
        }

        public static implicit operator JsonValue(_Subscribers subscribers)
        {
            return subscribers._json;
        }

        public override string ToString()
        {
            return _json.ToString();
        }
    }

    struct _Url
    {
        dynamic _json;
        internal _Url(string url) 
        {
            _json = new JsonObject();
            _json.url = url;
        }

        public static implicit operator JsonValue(_Url url)
        {
            return url._json;
        }

        public override string ToString()
        {
            return _json.ToString();
        }
    }

    public struct PushInfo
    {
        dynamic _json;

        public PushInfo(JsonValue json)
        {
            _json = json;
        }

        public long RetriesDelay { get { return _json.retries_delay; } set { _json.retries_delay = value; } }
        public long Retriesremaining { get { return _json.retries_remaining; } set { _json.retries_remaining = value; } }
        public int StatusCode { get { return _json.status_code; } set { _json.status_code = value; } }
        public string Url { get { return _json.url; } set { _json.url = value; } }
        public long ID { get { return _json.id; } set { _json.id = value; } }

        public override string ToString()
        {
            return _json.ToString();
        }
    }

    struct _Messages
    {
        dynamic _json;

        internal _Messages(string json)
        {
            _json = JsonObject.Parse(json);
        }

        internal _Messages(params Message[] messages)
        {
            _json = new JsonObject();
            _json.messages = new JsonArray(messages.Select(message => (JsonValue)message));
        }

        public Message[] Messages
        {
            get 
            { 
                return (_json.messages as IEnumerable<JsonValue>)
                    .Select(message => new Message(message)).ToArray(); 
            }
        }

        public static implicit operator JsonValue(_Messages messages)
        {
            return messages._json;
        }

        public override string ToString()
        {
            return _json.ToString();
        }

    }

    /// <summary>
    /// A message retrieved from an IronMQ queue.
    /// </summary>
    public struct Message
    {
        dynamic _json;

        internal Message(JsonValue json)
        {
            _json = json;
        }

        public Message(string body, long timeout = 60, long delay = 0, long expires = 604800):this(new JsonObject())
        {
            this.Body = body;
            this.Timeout = timeout;
            this.Delay = delay;
            this.Expires = expires;
            this.ID = 0; // unsubmitted message
        }

        public string Body { get { return _json.body; } set { _json.body = value; } }
        public long Timeout { get { return _json.timeout; } set { _json.timeout = value; } }
        public long Delay { get { return _json.delay; } set { _json.delay = value; } }
        public long Expires { get { return _json.expires_in; } set { _json.expires_in = value; } }
        public long ID { get { return _json.id; } internal set {  _json.id = value; } }

        Message Trim()
        {
            if (_json.ContainsKey("timeout") && this.Timeout == 60) _json.Remove("timeout");
            if (_json.ContainsKey("delay") && this.Delay == 60) _json.Remove("delay");
            if (_json.ContainsKey("expires_in") && this.Expires == 604800) _json.Remove("expires_in");
            if (_json.ContainsKey("id") && this.ID == 0) _json.Remove("id");
            return this;
        }

        public static implicit operator JsonValue(Message message)
        {
            return message.Trim()._json;
        }

        public override string ToString()
        {
            return ((JsonValue)this).ToString();
        }
    }

    struct _IDs
    {
        dynamic _json;
        internal _IDs(string json) { _json = JsonValue.Parse(json); }
        public IEnumerable<long> IDs { get { return (_json.ids as IEnumerable<JsonValue>).Select(id => (long)id); } }

        public override string ToString()
        {
            return _json.ToString();
        }
    }

    struct _Size
    {
        dynamic _json;
        internal _Size(string json) { _json = JsonValue.Parse(json); }
        public long Size { get { return _json.size; } }

        public override string ToString()
        {
            return _json.ToString();
        }
    }

    struct _QueueInfo
    {
        dynamic _json;

        internal _QueueInfo(JsonValue json) { _json = json; }

        public string ID { get { return _json.id; } }
        public string Name { get { return _json.name; } }
        public string ProjectID { get { return _json.project_id; } }

        public override string ToString()
        {
            return _json.ToString();
        }
    }
}
