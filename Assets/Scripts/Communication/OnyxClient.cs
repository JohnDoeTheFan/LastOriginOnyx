using System;
using System.Collections;
using UnityEngine;
using Onyx.Communication.Protocol;
using System.Collections.Generic;

namespace Onyx.Communication
{
    public class OnyxClient
    {
        public MonoBehaviour coroutineProxy;

        readonly CommunicationLayer communicationLayer;

        readonly IRetryConfirm retryConfirm;
        readonly IQuitAlert quitAlert;

        public OnyxClient(CommunicationLayer communicationLayer, MonoBehaviour coroutineProxy, IRetryConfirm retryConfirm, IQuitAlert quitAlert)
        {
            this.communicationLayer = communicationLayer;
            communicationLayer.OnConnectionFailed = OnConnectionFail;
            communicationLayer.OnCommunicationFailed = OnCommunicationFail;
            this.coroutineProxy = coroutineProxy;
            this.retryConfirm = retryConfirm;
            this.quitAlert = quitAlert;
        }

        public void CacheUid(UidRequest signInRequest, Action<int> onSuccess)
        {
            coroutineProxy.StartCoroutine(
                 communicationLayer.Communicate<UidRequest, UidResponse>(signInRequest, OnOk)
                );

            void OnOk(UidResponse signInResponse)
            {
                UserRequest.uidCache = signInResponse.uid;
                onSuccess(signInResponse.uid);
            }
        }

        public void GetIsStageCleared(IsStageClearedRequest isStageClearedRequest, Action<bool> callBack)
        {
            coroutineProxy.StartCoroutine(
                communicationLayer.Communicate<IsStageClearedRequest, IsStageClearedResponse>(isStageClearedRequest, OnOk)
                );

            void OnOk(IsStageClearedResponse isStageClearedResponse)
            {
                callBack(isStageClearedResponse.isCleared);
            }
        }

        public void SetIsStageCleared(SetStageClearedRequest setStageClearedRequest, Action<bool> callBack)
        {
            coroutineProxy.StartCoroutine(
                communicationLayer.Communicate<SetStageClearedRequest, SetStageClearedResponse>(setStageClearedRequest, OnOk)
                );

            void OnOk(SetStageClearedResponse setStageClearedResponse)
            {
                callBack(setStageClearedResponse.isCleared);
            }
        }

        public void LoadGreeting(LoadGreetingRequest loadGreetingRequest, Action<Vector2, float> callBack)
        {
            coroutineProxy.StartCoroutine(
                communicationLayer.Communicate<LoadGreetingRequest, LoadGreetingResponse>(loadGreetingRequest, OnOk)
                );

            void OnOk(LoadGreetingResponse loadGreetingResponse)
            {
                callBack(loadGreetingResponse.position, loadGreetingResponse.size);
            }
        }

        public void SaveGreeting(SaveGreetingRequest saveGreetingRequest, Action<bool> callBack)
        {
            coroutineProxy.StartCoroutine(
                communicationLayer.Communicate<SaveGreetingRequest, SaveGreetingResponse>(saveGreetingRequest, OnOk)
                );

            void OnOk(SaveGreetingResponse saveGreetingResponse)
            {
                callBack(saveGreetingResponse.isSuccess);
            }
        }

        public void GetOnyxValue(GetOnyxValueRequest getOnyxValueRequest, Action<int> callBack)
        {
            coroutineProxy.StartCoroutine(
                communicationLayer.Communicate<GetOnyxValueRequest, GetOnyxValueResponse>(getOnyxValueRequest, OnOk)
                );

            void OnOk(GetOnyxValueResponse getOnyxValueResponse)
            {
                callBack.Invoke(getOnyxValueResponse.onyxValue);
            }
        }

        public void AddOnyxValue(AddOnyxValueRequest addOnyxValueRequest, Action<int> callBack)
        {
            coroutineProxy.StartCoroutine(
                communicationLayer.Communicate<AddOnyxValueRequest, AddOnyxValueResponse>(addOnyxValueRequest, OnOk)
                );

            void OnOk(AddOnyxValueResponse addOnyxValueResponse)
            {
                callBack.Invoke(addOnyxValueResponse.onyxValue);
            }
        }

        public void GetPlayerLevel(GetPlayerLevelRequest getPlayerLevelRequest, Action<int> callBack)
        {
            coroutineProxy.StartCoroutine(
                communicationLayer.Communicate<GetPlayerLevelRequest, GetPlayerLevelResponse>(getPlayerLevelRequest, OnOk)
                );

            void OnOk(GetPlayerLevelResponse getPlayerLevelResponse)
            {
                callBack.Invoke(getPlayerLevelResponse.level);
            }
        }

        public void LevelUpPlayer(LevelUpPlayerRequest levelUpPlayerRequest, Action<int, int> callBack)
        {
            coroutineProxy.StartCoroutine(
                communicationLayer.Communicate<LevelUpPlayerRequest, LevelUpPlayerResponse>(levelUpPlayerRequest, OnOk)
                );

            void OnOk(LevelUpPlayerResponse levelUpPlayerResponse)
            {
                callBack.Invoke(levelUpPlayerResponse.level, levelUpPlayerResponse.onyxValue);
            }
        }

        public void UnlockCombatantEmbargo(UnlockCombatantEmbargoRequest unlockCombatantEmbargoRequest, Action<bool, int> callBack)
        {
            coroutineProxy.StartCoroutine(
                communicationLayer.Communicate<UnlockCombatantEmbargoRequest, UnlockCombatantEmbargoResponse>(unlockCombatantEmbargoRequest, OnOk)
                );

            void OnOk(UnlockCombatantEmbargoResponse unlockCombatantEmbargoResponse)
            {
                callBack.Invoke(unlockCombatantEmbargoResponse.isSucceed, unlockCombatantEmbargoResponse.onyxValue);
            }
        }

        public void GetOwningBioroidsIds(GetOwningBioroidsIdsRequest unlockCombatantEmbargoRequest, Action<List<int>> callBack)
        {
            coroutineProxy.StartCoroutine(
                communicationLayer.Communicate<GetOwningBioroidsIdsRequest, GetOwningBioroidsIdsResponse>(unlockCombatantEmbargoRequest, OnOk)
                );

            void OnOk(GetOwningBioroidsIdsResponse getOwningBioroidsIdsResponse)
            {
                callBack.Invoke(getOwningBioroidsIdsResponse.owningBioroidsIds);
            }
        }

        public void GetAideBioroidId(GetAideBioroidIdRequest getAideBioroidIdRequest, Action<int> callBack)
        {
            coroutineProxy.StartCoroutine(
                communicationLayer.Communicate<GetAideBioroidIdRequest, GetAideBioroidIdResponse>(getAideBioroidIdRequest, OnOk)
                );

            void OnOk(GetAideBioroidIdResponse getAideBioroidIdResponse)
            {
                callBack(getAideBioroidIdResponse.aideBioroidId);
            }
        }

        public void SetAideBioroidId(SetAideBioroidIdRequest setAideBioroidIdRequest, Action<bool> callBack)
        {
            coroutineProxy.StartCoroutine(
                communicationLayer.Communicate<SetAideBioroidIdRequest, SetAideBioroidIdResponse>(setAideBioroidIdRequest, OnOk)
                );

            void OnOk(SetAideBioroidIdResponse setAideBioroidIdResponse)
            {
                callBack(setAideBioroidIdResponse.isSuccess);
            }

        }

        private void OnCommunicationFail(Func<IEnumerator> tryAgain, int retryCount)
        {
            Debug.Log("Communication is failed. retry count: " + retryCount);
            if (retryCount < 3)
                coroutineProxy.StartCoroutine(NotifyThenCallTryAgain(tryAgain));
            else
                OnConnectionFail(CommunicationLayer.ConnectingStatus.RetryOut);

            IEnumerator NotifyThenCallTryAgain(Func<IEnumerator> tryAgain)
            {
                yield return coroutineProxy.StartCoroutine(retryConfirm.Confirm());
                if (retryConfirm.Result)
                    coroutineProxy.StartCoroutine(tryAgain());
                else
                    OnConnectionFail(CommunicationLayer.ConnectingStatus.RetryOut);
            }
        }

        private void OnConnectionFail(VirtualCommunication.ConnectingStatus status)
        {
            string reason = status switch
            {
                CommunicationLayer.ConnectingStatus.Connected => "",
                CommunicationLayer.ConnectingStatus.ServerBusy => "ServerBusy",
                CommunicationLayer.ConnectingStatus.ServerMaintaining => "ServerMaintaing",
                CommunicationLayer.ConnectingStatus.RetryOut => "RetryOut",
                CommunicationLayer.ConnectingStatus.NoServer => "NoServer",
                _ => "Undefined",
            };

            Debug.Log("Connection is failed(" + reason + "). Quit application.");
            coroutineProxy.StartCoroutine(quitAlert.Alert());
        }

        public interface IRetryConfirm
        {
            IEnumerator Confirm();
            bool Result { get; }
        }
        public interface IQuitAlert
        {
            IEnumerator Alert();
        }
    }
}
