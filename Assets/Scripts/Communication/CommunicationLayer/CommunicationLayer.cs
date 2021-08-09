using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.Communication
{
    public abstract class CommunicationLayer
    {
        public Action<ConnectingStatus> OnConnectionFailed;
        public Action<Func<IEnumerator>, int> OnCommunicationFailed;

        public enum ConnectingStatus
        {
            Connected,
            ServerBusy,
            ServerMaintaining,
            RetryOut,
            NoServer
        }

        public enum ErrorCode
        {
            Undefined,
            Disconnected,
            Timeout,
        }

        public abstract IEnumerator Communicate<Request, Response>(Request request, Action<Response> onOk);
    }
}