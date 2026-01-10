using UnityEngine;

public static class Utils
{

    public static bool IsValid(Vector3 v)
    {
        return
            !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z) &&
            !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z);
    }

    public static Vector3 ExpDecay(Vector3 a, Vector3 b, float decay, float deltaTime)
    {
        return new Vector3(
            ExpDecay(a.x, b.x, decay, deltaTime),
            ExpDecay(a.y, b.y, decay, deltaTime),
            ExpDecay(a.z, b.z, decay, deltaTime)
        );
    }

    public static float ExpDecay(float a, float b, float decay, float time)
    {
        if (time < 0)
        {
            return a;
        }

        if (decay <= 0)
        {
            return b;
        }

        return Mathf.Lerp(a, b, 1 - Mathf.Exp(-decay * time));
    }
}
