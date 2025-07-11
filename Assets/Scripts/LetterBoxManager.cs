using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterBoxManager : MonoBehaviour
{

    LetterBoxer lb;


    private void Awake()
    {
        lb = GetComponent<LetterBoxer>();
#if UNITY_ANDROID || UNITY_IOS
        lb.enabled = true;
#else
        lb.enabled = false;

#endif
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);

        if (lb.enabled)
            lb.PerformSizing();
    }

}

