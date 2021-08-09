using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillSystem
{
    public enum Team
    {
        All,
        Player,
        Enemy
    }
    public enum Equality
    {
        Equal,
        EqualOrLess,
        EqualOrGeater,
        Less,
        Greater
    }

    public static class ExtensionMethods
    {
        public static bool Check(this Equality eq, int op0, int op1)
        {
            return eq switch
            {
                Equality.Equal => op0 == op1,
                Equality.EqualOrLess => op0 <= op1,
                Equality.EqualOrGeater => op0 >= op1,
                Equality.Less => op0 < op1,
                Equality.Greater => op0 > op1,
                _ => false
            };
        }
    }

    public interface ICondition
    {
        bool Check(AbstractiveObject obj);
    }

    [System.Serializable]
    public class StatisticCondition : ICondition
    {
        public enum Statistic
        {
            Durability,
            Strength
        }
        public Statistic statistic;
        public Equality equality;
        public float value;

        bool ICondition.Check(AbstractiveObject obj)
        {
            return true;
        }
    }

    [System.Serializable]
    public class SkillCondition : ICondition
    {
        public AbstractiveSkill skill;
        public Equality equality;
        public int level;
        bool ICondition.Check(AbstractiveObject obj)
        {
            foreach (AbstractiveSkill objSkill in obj.Skills)
            {
                if (objSkill == skill && equality.Check(objSkill.level, level))
                        return true;
            }
            return false;
        }
    }

    [System.Serializable]
    public class BuffCondition : ICondition
    {
        public AbstractiveBuff buff;
        public Equality equality;
        public int stackCount;
        bool ICondition.Check(AbstractiveObject obj)
        {
            foreach (AbstractiveBuff objBuff in obj.Buffs)
            {
                if (objBuff == buff && equality.Check(objBuff.stackCount, stackCount))
                        return true;
            }
            return false;
        }
    }

    public abstract class AbstractiveObject
    {
        public Team Team { get; }

        public List<AbstractiveSkill> Skills { get; }

        public List<AbstractiveBuff> Buffs { get; }

        public bool CheckConditions(List<ICondition> conditions)
        {
            foreach (ICondition condition in conditions)
            {
                if ( ! condition.Check(this))
                    return false;
            }
            return true;
        }
    }

    public abstract class AbstractiveTiming : ScriptableObject
    {

    }

    public abstract class AbstractiveSkill : ScriptableObject
    {
        public int level;
    }

    public abstract class AbstractiveFilter : ScriptableObject
    {
        public virtual List<AbstractiveObject> FilterObjects(List<AbstractiveObject> filter)
        {
            return new List<AbstractiveObject>();
        }
    }

    public abstract class AbstractiveBuff : ScriptableObject
    {
        public int stackCount;
    }

}