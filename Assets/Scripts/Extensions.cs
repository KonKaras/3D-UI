using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static bool IsCloseTo(this Vector3 self, Vector3 other)
	{
		return (self - other).magnitude < 0.01f;
	}
}