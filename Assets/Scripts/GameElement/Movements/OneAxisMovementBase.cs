using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OneAxisMovementBase : MovementBase
{
    public override Vector2 CullDirection(Vector2 leftStickInput)
    {
        if (leftStickInput.x > 0)
            return Vector2.right;
        else if (leftStickInput.x < 0)
            return Vector2.left;
        else
            return Vector2.zero;
    }
}
