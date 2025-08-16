using System;
using Godot;

namespace Jomolith.Scripts.Utils;

public static class MathUtils
{
    public static float GetMedian(float[] sourceNumbers) {
        //Framework 2.0 version of this method. there is an easier way in F4        
        if (sourceNumbers == null || sourceNumbers.Length == 0)
            throw new System.Exception("Median of empty array not defined.");

        //make sure the list is sorted, but use a new array
        float[] sortedPNumbers = (float[])sourceNumbers.Clone();
        Array.Sort(sortedPNumbers);

        //get the median
        int size = sortedPNumbers.Length;
        int mid = size / 2;
        float median = (size % 2 != 0) ? sortedPNumbers[mid] : (sortedPNumbers[mid] + sortedPNumbers[mid - 1]) / 2;

        return median;
    }

    public static Vector3 RemoveAngularComponent(this Vector3 omega, Vector3 axisToRemove)
    {
        Vector3 normalized = axisToRemove.Normalized();
        Vector3 projection = omega.Dot(normalized) * normalized;
        return omega - projection;
    }
}