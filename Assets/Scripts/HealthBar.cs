using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    public List<GameObject> healthPoints = new List<GameObject>();

    public Sprite redBar;
    public Sprite greenBar;


    public void SetHealth(int value)//Set Health Bar UI based on Relationship points. Red <=3, Green >=4
    {

        foreach (GameObject item in healthPoints)
        {
            item.SetActive(false);
        }

        for (int i = 0; i < value; i++)
        {
            healthPoints[i].SetActive(true);

            if (value > 3)
            {
                healthPoints[i].GetComponent<Image>().sprite = greenBar;
            }
            else
            {
                healthPoints[i].GetComponent<Image>().sprite = redBar;
            }

        }
    }
}
