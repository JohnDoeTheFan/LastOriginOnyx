using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx.Communication.Protocol;

namespace Onyx.Communication
{
    public class AssetReadCommunication : CommunicationLayer
    {
        readonly private SaveDataAsset saveDataAsset;

        public AssetReadCommunication(SaveDataAsset saveDataAsset)
        {
            this.saveDataAsset = saveDataAsset;
        }

        public override IEnumerator Communicate<Request, Response>(Request request, Action<Response> onOk)
        {
            yield return NetworkDelay();

            string requestJsonString = JsonUtility.ToJson(request);

            RequestBase requestBase = JsonUtility.FromJson<RequestBase>(requestJsonString);

            object responseObject = null;
            if (requestBase.type == RequestType.SignIn)
            {
                responseObject = new UidResponse(1234567);
            }
            else if (requestBase.type == RequestType.IsStageCleared)
            {
                IsStageClearedRequest isStageClearedRequest = JsonUtility.FromJson<IsStageClearedRequest>(requestJsonString);

                bool isCleared = saveDataAsset.IsStageCleared(isStageClearedRequest.chapterNum, isStageClearedRequest.stageNum, isStageClearedRequest.stageType);
                Debug.Log(isStageClearedRequest.chapterNum + ", " + isStageClearedRequest.stageNum + ", " + isStageClearedRequest.stageType + " is" + (isCleared ? "" : " not") + " cleared.");

                responseObject = new IsStageClearedResponse(isCleared);
            }
            else if (requestBase.type == RequestType.SetStageCleared)
            {
                SetStageClearedRequest setStageClearedRequest = JsonUtility.FromJson<SetStageClearedRequest>(requestJsonString);

                saveDataAsset.SetStageCleared(setStageClearedRequest.chapterNum, setStageClearedRequest.stageNum, setStageClearedRequest.stageType);
                Debug.Log("cleared " + setStageClearedRequest.chapterNum + ", " + setStageClearedRequest.stageNum + ", " + setStageClearedRequest.stageType + ".");

                responseObject = new SetStageClearedResponse(true);
            }
            else if (requestBase.type == RequestType.LoadGreeting)
            {
                responseObject = new LoadGreetingResponse(saveDataAsset.GreetingPosition, saveDataAsset.GreetingSize);
            }
            else if (requestBase.type == RequestType.SaveGreeting)
            {
                SaveGreetingRequest saveGreetingRequest = JsonUtility.FromJson<SaveGreetingRequest>(requestJsonString);

                saveDataAsset.SetGreetingData(saveGreetingRequest.position, saveGreetingRequest.size);
                Debug.Log("Saved greeting camera " + saveGreetingRequest.position + ", " + saveGreetingRequest.size + ".");

                responseObject = new SaveGreetingResponse(true);
            }
            else if (requestBase.type == RequestType.GetOnyxValue)
            {
                GetOnyxValueRequest getOnyxRequest = JsonUtility.FromJson<GetOnyxValueRequest>(requestJsonString);

                Debug.Log("OnxyValue: " + saveDataAsset.OnyxValue);

                responseObject = new GetOnyxValueResponse(saveDataAsset.OnyxValue);
            }
            else if(requestBase.type == RequestType.AddOnyxValue)
            {
                AddOnyxValueRequest addOnyxRequest = JsonUtility.FromJson<AddOnyxValueRequest>(requestJsonString);

                int lastOnyxValue = saveDataAsset.OnyxValue;
                saveDataAsset.AddOnyx(addOnyxRequest.onyxValue);
                Debug.Log("OnyxValue: " + lastOnyxValue + " + " + addOnyxRequest.onyxValue + " = " + saveDataAsset.OnyxValue);

                responseObject = new AddOnyxValueResponse(saveDataAsset.OnyxValue);
            }
            else if (requestBase.type == RequestType.GetPlayerLevel)
            {
                GetPlayerLevelRequest getPlayerLevelRequest = JsonUtility.FromJson<GetPlayerLevelRequest>(requestJsonString);

                Debug.Log("PlayerLevel: " + saveDataAsset.PlayerLevel);

                responseObject = new GetPlayerLevelResponse(saveDataAsset.PlayerLevel);

            }
            else if (requestBase.type == RequestType.LevelUpPlayer)
            {
                LevelUpPlayerRequest levelUpPlayerRequest = JsonUtility.FromJson<LevelUpPlayerRequest>(requestJsonString);

                int lastPlayerLevel = saveDataAsset.PlayerLevel;
                int lastOnxyValue = saveDataAsset.OnyxValue;
                int lastOnxyCost = saveDataAsset.GetLevelUpCost();
                if (saveDataAsset.LevelUpPlayer())
                {
                    Debug.Log("PlayerLevel: " + lastPlayerLevel + " + 1 = " + saveDataAsset.PlayerLevel);
                    Debug.Log("LevelUpCost: " + lastOnxyValue + " - " + lastOnxyCost + " = " + saveDataAsset.OnyxValue);
                }
                else
                {
                    Debug.Log("LevelUpPlayer failed.");
                }

                responseObject = new LevelUpPlayerResponse(saveDataAsset.PlayerLevel, saveDataAsset.OnyxValue);
            }

            onOk(JsonUtility.FromJson<Response>(JsonUtility.ToJson(responseObject)));
            yield break;
        }

        private IEnumerator NetworkDelay()
        {
            Vector2 networkDelay = saveDataAsset.NetworkDelay;
            networkDelay.x = Mathf.Max(0, networkDelay.x);
            networkDelay.y = Mathf.Max(0, networkDelay.y);

            if (networkDelay.x == networkDelay.y)
            {
                yield return new WaitForSecondsRealtime(networkDelay.x);
            }
            else
            {
                float networkDelayMin = Mathf.Min(networkDelay.x, networkDelay.y);
                float networkDelayMax = Mathf.Max(networkDelay.x, networkDelay.y);

                float randomNetworkDelay = UnityEngine.Random.Range(networkDelayMin, networkDelayMax);

                yield return new WaitForSecondsRealtime(randomNetworkDelay);
            }
        }
    }
}