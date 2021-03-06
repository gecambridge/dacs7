﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Dacs7
{
    public class Message : IMessage
    {
        private static int _idSeqGen;
        private readonly string _id;
        private string _origin = string.Empty;
        private readonly DateTime _creationTime;
        private readonly Dictionary<string, object> _attributes = new Dictionary<string, object>();
        public Dictionary<string, object> Attributes { get { return new Dictionary<string, object>(_attributes); } }
        private IEnumerable<byte> _rawMessage;
        private IProtocolPolicy _protocolPolicy;


        private static string CreateID()
        {
            var seq = Interlocked.Increment(ref _idSeqGen);
            var sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("yyyymmdd HHMMss"));
            sb.AppendFormat(" {0:X4}", seq);
            return sb.ToString();
        }

        protected Message()
        {
            _creationTime = DateTime.Now;
            _id = CreateID();
        }


        public static IMessage Create()
        {
            return new Message();
        }

        public static IMessage CreateFromRawMessage(string origin, IProtocolPolicy protocolPolicy, IEnumerable<byte> rawMessage)
        {
            var message = new Message { _protocolPolicy = protocolPolicy, _rawMessage = rawMessage };
            return message;
        }


        public IMessage GetReplyMessage()
        {
            return null;
        }

        public T GetAttribute<T>(string attributeName, T defaultValue)
        {
            if (_attributes.TryGetValue(attributeName, out object value))
                return (T)value;
            return defaultValue;
        }

        public void SetAttribute(string attributeName, object attributeValue)
        {
            _attributes[attributeName] = attributeValue;
        }

        public void RemoveAttribute(string attributeName)
        {
            _attributes.Remove(attributeName);
        }

        public string GetID()
        {
            return _id;
        }

        public string GetOrigin()
        {
            return _origin;
        }

        public IEnumerable<byte> GetRawMessage()
        {
            return _rawMessage;
        }

        public object GetPayload()
        {
            var data = (_rawMessage as IEnumerable<byte>);
            if (data == null)
            {
                if (_attributes.TryGetValue("$$Payload", out object obj))
                    data = (obj as IEnumerable<byte>);
                return data == null ? new List<byte>() : new List<byte>(data);
            }
            if (_attributes.TryGetValue("$$PayloadOffset", out object offset) && _attributes.TryGetValue("$$PayloadLength", out object length))
                return new List<byte>(data.Skip((int)offset).Take((int)length));

            return new List<byte>();
        }

        public int GetAge()
        {
            return (int)DateTime.Now.Subtract(_creationTime).TotalSeconds;
        }


        public IProtocolPolicy GetProtocolPolicy()
        {
            return _protocolPolicy;
        }

    }

}
