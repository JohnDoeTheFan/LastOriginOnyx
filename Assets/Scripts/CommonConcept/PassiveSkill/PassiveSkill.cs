using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SkillSystem
{
    [CreateAssetMenu(menuName = "Unused/PassiveSkill")]
    public class PassiveSkill : AbstractiveSkill
    {
        [System.Serializable]
        private class Functionality
        {
            public AbstractiveTiming timing = null;
            public AbstractiveFilter filter;
            public AbstractiveBuff buff;
        }

        [SerializeField]
        private List<Functionality> functionalities;
    }
}
