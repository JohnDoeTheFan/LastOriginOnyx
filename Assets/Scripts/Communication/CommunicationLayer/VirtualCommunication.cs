using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.Communication
{
    public class VirtualCommunication : CommunicationLayer
    {
        private Connection connection = null;

        private ConnectingResult Connect()
        {
            return new ConnectingResult(ConnectingStatus.Connected, new Connection());
        }

        public override IEnumerator Communicate<Request, Response>(Request request, Action<Response> onOk)
        {
            return Communicate(request, onOk, 0);
        }

        private IEnumerator Communicate<Request, Response>(Request request, Action<Response> onOk, int retryCount)
        {
            if (connection == null || connection.shouldRepublish)
            {
                ConnectingResult result = Connect();
                if (result.status == ConnectingStatus.Connected)
                    connection = result.connection;
                else
                    OnConnectionFailed(result.status);
            }

            Connection.Transaction newTransaction = connection.GetTransaction();
            yield return newTransaction.Send(JsonUtility.ToJson(request));

            if (newTransaction.Result.status == Connection.Transaction.ResultStruct.Status.Ok)
                onOk(JsonUtility.FromJson<Response>(newTransaction.Result.message));
            else
                OnCommunicationFailed(() => { return Communicate(request, onOk, retryCount + 1); }, retryCount + 1);
        }

        public class Connection
        {
            public bool shouldRepublish = false;

            public Transaction GetTransaction()
            {
                return new Transaction(() => shouldRepublish = true);
            }

            public class Transaction
            {
                private ResultStruct result = new ResultStruct();

                readonly private Action OnConnectionProblem;
                public ResultStruct Result => result;

                public Transaction(Action OnConnectionProblem)
                {
                    this.OnConnectionProblem = OnConnectionProblem;
                }

                public IEnumerator Send(string message)
                {
                    return new TransactionAsyncOperation(OnComplete, OnError);
                }

                public struct ResultStruct
                {
                    public ResultStruct(Status status, string message, ErrorCode errorCode)
                    {
                        this.status = status;
                        this.errorCode = errorCode;
                        this.message = message;
                    }

                    public Status status;
                    public ErrorCode errorCode;
                    public string message;
                    public enum Status
                    {
                        Ok,
                        Error
                    }
                }

                private void OnComplete(string data)
                {
                    result.status = ResultStruct.Status.Ok;
                    result.message = data;
                    result.errorCode = ErrorCode.Undefined;
                }

                private void OnError(ErrorCode errorCode)
                {
                    result.status = ResultStruct.Status.Error;
                    result.message = "";
                    result.errorCode = errorCode;
                    OnConnectionProblem();
                }

                class TransactionAsyncOperation : CustomYieldInstruction
                {
                    readonly private float startTime;
                    readonly private float timeOut = 1f;
                    private string data = string.Empty;
                    readonly private Action<string> OnComplete;
                    readonly private Action<ErrorCode> OnError;

                    public TransactionAsyncOperation(Action<string> OnComplete, Action<ErrorCode> OnError)
                    {
                        startTime = Time.time;
                        this.OnComplete = OnComplete;
                        this.OnError = OnError;
                    }

                    public override bool keepWaiting => CheckKeepWaiting();

                    public bool CheckKeepWaiting()
                    {
                        data += ReadDataFromStream();
                        if (data.Length != 0 && data[data.Length - 1] == '\0')
                        {
                            OnComplete(data);
                            return false;
                        }

                        if (Time.time - startTime > timeOut)
                        {
                            OnError(ErrorCode.Timeout);
                            return false;
                        }

                        return true;
                    }

                    public string ReadDataFromStream()
                    {
                        /*
                        OnyxClient.SignInResponse test = new OnyxClient.SignInResponse();
                        test.uid = 1234567;
                        return JsonUtility.ToJson(test) + "\0";
                        */
                        return "";
                    }
                }
            }
        }
        public struct ConnectingResult
        {
            public ConnectingResult(ConnectingStatus status, Connection connection)
            {
                this.status = status;
                this.connection = connection;
            }
            public ConnectingStatus status;
            public Connection connection;
        }

    }
}