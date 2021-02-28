using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public static class UnityExtensions
    {
        private static bool isCoroutineExecuting = false;

        public static IEnumerator ExecuteAfterTime(float seconds)
        {
            if (isCoroutineExecuting)
                yield break;
            isCoroutineExecuting = true;
            yield return new WaitForSeconds(seconds);
            isCoroutineExecuting = false;
        }
    }
}
