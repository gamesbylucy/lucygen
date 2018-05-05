using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConversionUtilities {

    public static Vector3 toUnitCircle(Vector3 point)
    {
        Vector3 result = Vector3.zero;
        float distToOrigin = Vector3.Distance(point, Vector3.zero);
        result *= distToOrigin;
        return result;
    }

    public static Vector3 ToCartesian(Vector3 aSpherical)
    {
        return ToCartesian(aSpherical, Quaternion.identity);
    }
    public static Vector3 ToCartesian(Vector3 aSpherical, Quaternion aSpace)
    {
        Vector3 result;
        float c = Mathf.Cos(aSpherical.y);
        result.x = Mathf.Cos(aSpherical.x) * c;
        result.y = Mathf.Sin(aSpherical.y);
        result.z = Mathf.Sin(aSpherical.x) * c;
        result *= aSpherical.z;
        result = aSpace * result;
        return result;
    }

    public static Vector3 ToCircle(Vector3 aSpherical, float aRadius)
    {
        Vector3 result;
        float r = -aRadius * (aSpherical.y - Mathf.PI / 2) / Mathf.PI;
        result.x = Mathf.Cos(aSpherical.x) * r;
        result.y = aSpherical.z;
        result.z = Mathf.Sin(aSpherical.x) * r;
        return result;
    }
}
