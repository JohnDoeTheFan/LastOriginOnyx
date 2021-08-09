using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Utility/MultiplierPerLevel")]
public class MultiplierPerLevel : ScriptableObject
{
    [SerializeField]
    private List<LevelAndMultiplier> levelAndMultipliers;

    [System.Serializable]
    struct LevelAndMultiplier
    {
        public int level;
        public float multiplier;
    }

    public float GetMultiplier(int level)
    {
        if (levelAndMultipliers.Count == 0)
            return 0;

        float retVal = 0;
        foreach(LevelAndMultiplier pair in levelAndMultipliers)
        {
            if (level >= pair.level)
                retVal = pair.multiplier;
        }
        return retVal;
    }
}
