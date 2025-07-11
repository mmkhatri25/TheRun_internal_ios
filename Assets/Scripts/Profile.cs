using UnityEngine;
using UnityEngine.UI;

public class Profile : MonoBehaviour
{

    public Sprite defaultSprite;
    public Image characterImage;
    public bool isDead;


    private void Awake()
    {
        defaultSprite = characterImage.sprite;
    }

    public void SetDefaultSprite()
    {
        characterImage.sprite = defaultSprite;
    }


    public void SetCharacter(string name, string statusType) //Load character images from Resources folder and set color for it
    {
        Sprite sp = null;
        Debug.Log(name + "Death" + "   " + statusType);

        switch (statusType)
        {
            case "alive":
                isDead = false;
                //sp = Resources.Load<Sprite>("CharacterProfiles/" + name);
                break;
            case "death":
                Debug.Log("Death CASe working");
                isDead = true;
                //sp = Resources.Load<Sprite>("CharacterProfiles/" + name + "Death");
                break;
            case "fail":
                //sp = Resources.Load<Sprite>("CharacterProfiles/" + name + "Fail");
                break;
            case "success":
                //sp = Resources.Load<Sprite>("CharacterProfiles/" + name + "Success");
                break;
            case "revival":
                //sp = Resources.Load<Sprite>("CharacterProfiles/" + name + "Revival");
                break;
        }


        if (sp != null)
        {
            characterImage.sprite = sp;
        }
    }
}
