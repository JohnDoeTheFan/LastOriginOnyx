using NUnit.Framework;
using UnityEngine;

public class OnyxOrbGuiTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void RandomVector2()
    {
        float minDegree = 179;
        float maxDegree = 180;

        for(int i = 0; i < 1000; i++)
        {
            Vector2 newVector = OnyxOrbGui.MakeRandomNormalVector2(minDegree, maxDegree);
            float angle = Vector2.SignedAngle(Vector2.right, newVector);

            Debug.Log(newVector + ", " + angle);

            UnityEngine.Assertions.Assert.AreApproximatelyEqual(1, newVector.magnitude);
            Assert.GreaterOrEqual(angle, minDegree);
            Assert.LessOrEqual(angle, maxDegree);
        }

    }
    [Test]
    public void DecreaseSpeed()
    {
        Vector2 speed = new Vector2(100, 100);

        Vector2 limitedSpeed = OnyxOrbGui.LimitVelocity(speed, 100);

        Debug.Log(speed + " to " + limitedSpeed);

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(100, limitedSpeed.magnitude);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(0, Vector2.Angle(limitedSpeed, speed));
    }

}
