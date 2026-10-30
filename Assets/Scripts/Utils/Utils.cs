using UnityEngine;

public static class Utils
{
    // Random index in [0, count), never equal to excludedIndex (unless it's the only one).
    public static int RandomIndexExcluding(int count, int excludedIndex)
    {
        if (count <= 1 || excludedIndex < 0 || excludedIndex >= count)
        {
            return Random.Range(0, count);
        }

        int index = Random.Range(0, count - 1);
        return index >= excludedIndex ? index + 1 : index;
    }


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

    public static Color ExpDecay(Color a, Color b, float decay, float deltaTime)
    {
        return new Color(
            ExpDecay(a.r, b.r, decay, deltaTime),
            ExpDecay(a.g, b.g, decay, deltaTime),
            ExpDecay(a.b, b.b, decay, deltaTime),
            ExpDecay(a.a, b.a, decay, deltaTime)
        );
    }

    public static Quaternion ExpDecay(Quaternion a, Quaternion b, float decay, float deltaTime)
    {
        if (deltaTime < 0)
        {
            return a;
        }
        if (decay <= 0)
        {
            return b;
        }
        return new Quaternion(
            ExpDecay(a.x, b.x, decay, deltaTime),
            ExpDecay(a.y, b.y, decay, deltaTime),
            ExpDecay(a.z, b.z, decay, deltaTime),
            ExpDecay(a.w, b.w, decay, deltaTime)
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
