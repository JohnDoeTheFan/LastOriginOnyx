using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillSystem
{
    [CreateAssetMenu(menuName = "Unused/SkillSystemFilter")]
    public class SkillSystemFilter : AbstractiveFilter
    {
        [SerializeField]
        private Team team;

        [SerializeField]
        private List<SkillCondition> skillConditions;

        [SerializeField]
        private List<StatisticCondition> statisticConditions;

        [SerializeField]
        private List<BuffCondition> buffConditions;

        private List<ICondition> Conditions { 
            get { 
                List<ICondition> retVal = new List<ICondition>(statisticConditions);
                retVal.AddRange(skillConditions);
                retVal.AddRange(buffConditions);
                return retVal;
            }
        }

        public override List<AbstractiveObject> FilterObjects(List<AbstractiveObject> objects)
        {
            List<AbstractiveObject> retVal = new List<AbstractiveObject>();

            foreach (AbstractiveObject obj in objects)
            {
                if (obj.Team != team)
                    continue;
                if ( ! obj.CheckConditions(Conditions))
                    continue;
            }
            return retVal;
        }

    }
}
