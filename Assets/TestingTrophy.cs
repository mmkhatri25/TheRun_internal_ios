using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingTrophy : MonoBehaviour
{
    #if UNITY_PS4
    int count = 0;

    TrophyManager tp;

    // Start is called before the first frame update
    void Start()
    {
        tp = FindObjectOfType<TrophyManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void OnButtonClick()
    {
        tp.UnlockTrophy(count);

        if (count < 16)
            count++;
    }
#endif
}
