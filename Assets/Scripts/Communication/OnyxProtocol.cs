using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Onyx.Communication.Protocol
{
    public enum RequestType
    {
        SignIn,
        IsStageCleared,
        SetStageCleared,
        LoadGreeting,
        SaveGreeting,
        GetOnyxValue,
        AddOnyxValue,
        GetPlayerLevel,
        LevelUpPlayer,
        UnlockCombatantEmbargo,
        GetOwningBioroidsIds,
    }

    [Serializable]
    public class RequestBase
    {
        public RequestBase(RequestType type)
        {
            this.type = type;
        }
        public RequestType type;
    }

    [Serializable]
    public class UserRequest : RequestBase
    {
        public static int uidCache;
        public UserRequest(RequestType type) : base(type)
        {
            uid = uidCache;
        }
        public int uid;
    }

    [Serializable]
    public class UidRequest : RequestBase
    {
        public UidRequest(string key) : base(RequestType.SignIn)
        {
            this.key = key;
        }
        public string key;
    }

    [Serializable]
    public class IsStageClearedRequest : UserRequest
    {
        public IsStageClearedRequest(int chapterNum, int stageNum, int stageType) : base(RequestType.IsStageCleared)
        {
            this.chapterNum = chapterNum;
            this.stageNum = stageNum;
            this.stageType = stageType;
        }
        public int chapterNum;
        public int stageNum;
        public int stageType;
    }

    [Serializable]
    public class SetStageClearedRequest : UserRequest
    {
        public SetStageClearedRequest(int chapterNum, int stageNum, int stageType) : base(RequestType.SetStageCleared)
        {
            this.chapterNum = chapterNum;
            this.stageNum = stageNum;
            this.stageType = stageType;
        }
        public int chapterNum;
        public int stageNum;
        public int stageType;
    }

    [Serializable]
    public class LoadGreetingRequest : UserRequest
    {
        public LoadGreetingRequest() : base(RequestType.LoadGreeting) { }
    }

    [Serializable]
    public class SaveGreetingRequest : UserRequest
    {
        public SaveGreetingRequest(Vector2 position, float size) : base(RequestType.SaveGreeting)
        {
            this.position = position;
            this.size = size;
        }
        public Vector2 position;
        public float size;
    }

    [Serializable]
    public class GetOnyxValueRequest : UserRequest
    {
        public GetOnyxValueRequest() : base(RequestType.GetOnyxValue) { }
    }

    [Serializable]
    public class AddOnyxValueRequest : UserRequest
    {
        public AddOnyxValueRequest(int onyxValue) : base(RequestType.AddOnyxValue) 
        {
            this.onyxValue = onyxValue;
        }
        public int onyxValue;
    }

    [Serializable]
    public class GetPlayerLevelRequest : UserRequest
    {
        public GetPlayerLevelRequest() : base(RequestType.GetPlayerLevel)
        {
        }
    }

    [Serializable]
    public class LevelUpPlayerRequest : UserRequest
    {
        public LevelUpPlayerRequest() : base(RequestType.LevelUpPlayer)
        {
        }
    }

    [Serializable]
    public class UnlockCombatantEmbargoRequest : UserRequest
    {
        public UnlockCombatantEmbargoRequest(int bioroidId) : base(RequestType.UnlockCombatantEmbargo)
        {
            this.bioroidId = bioroidId;
        }

        public int bioroidId;
    }

    [Serializable]
    public class GetOwningBioroidsIdsRequest : UserRequest
    {
        public GetOwningBioroidsIdsRequest() : base(RequestType.GetOwningBioroidsIds)
        {
        }
    }

    [Serializable]
    public struct UidResponse
    {
        public UidResponse(int uid)
        {
            this.uid = uid;
        }
        public int uid;
    }

    [Serializable]
    public struct IsStageClearedResponse
    {
        public IsStageClearedResponse(bool isCleared)
        {
            this.isCleared = isCleared;
        }
        public bool isCleared;
    }

    [Serializable]
    public struct SetStageClearedResponse
    {
        public SetStageClearedResponse(bool isCleared)
        {
            this.isCleared = isCleared;
        }
        public bool isCleared;
    }

    [Serializable]
    public struct LoadGreetingResponse
    {
        public LoadGreetingResponse(Vector2 position, float size)
        {
            this.position = position;
            this.size = size;
        }
        public Vector2 position;
        public float size;
    }

    [Serializable]
    public struct SaveGreetingResponse
    {
        public SaveGreetingResponse(bool isSuccess)
        {
            this.isSuccess = isSuccess;
        }
        public bool isSuccess;
    }

    [Serializable]
    public struct GetOnyxValueResponse
    {
        public GetOnyxValueResponse(int onyxValue)
        {
            this.onyxValue = onyxValue;
        }
        public int onyxValue;
    }

    [Serializable]
    public struct AddOnyxValueResponse
    {
        public AddOnyxValueResponse(int onyxValue)
        {
            this.onyxValue = onyxValue;
        }
        public int onyxValue;
    }

    [Serializable]
    public class GetPlayerLevelResponse
    {
        public GetPlayerLevelResponse(int level)
        {
            this.level = level;
        }
        public int level;
    }

    [Serializable]
    public class LevelUpPlayerResponse
    {
        public LevelUpPlayerResponse(int level, int onyxValue)
        {
            this.level = level;
            this.onyxValue = onyxValue;
        }
        public int level;
        public int onyxValue;
    }

    [Serializable]
    public class UnlockCombatantEmbargoResponse
    {
        public UnlockCombatantEmbargoResponse(bool isSucceed, int onyxValue)
        {
            this.isSucceed = isSucceed;
            this.onyxValue = onyxValue;
        }
        public bool isSucceed;
        public int onyxValue;
    }

    [Serializable]
    public class GetOwningBioroidsIdsResponse
    {
        public GetOwningBioroidsIdsResponse(List<int> owningBioroidsIds)
        {
            this.owningBioroidsIds = owningBioroidsIds;
        }
        public List<int> owningBioroidsIds;
    }
}
