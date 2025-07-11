using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatsCanvasManager : MonoBehaviour
{

    string profilePrefix;

    [SerializeField] private int totalEndings;
    [SerializeField] private int totalScenes;

    [Header("Male Character")]
    [SerializeField] private string profilePrefix_Male;
    [SerializeField] private int totalEndings_Male;
    [SerializeField] private int totalScenes_Male;
    [SerializeField] private Profile mainCharacter_Male;
    [SerializeField] private TextMeshProUGUI name_Male;
    [SerializeField] private GameObject male;
    public HealthBar maleHealthBar;


    [Header("FeMale Character")]
    [SerializeField] private string profilePrefix_female;
    [SerializeField] private int totalEndings_female;
    [SerializeField] private int totalScenes_female;
    [SerializeField] private Profile mainCharacter_Female;
    [SerializeField] private TextMeshProUGUI name_Female;
    [SerializeField] private GameObject female;
    public HealthBar femaleHealthBar;



    [Header("Text")]
    [SerializeField] private TextMeshProUGUI endingFound;
    [SerializeField] private TextMeshProUGUI scenesDiscovered;
    [SerializeField] private TextMeshProUGUI decisionsMade;

    //[Header("Colors")]
    //[SerializeField] private Color32 alive;
    //[SerializeField] private Color32 dead;

    [Space(20)]
    public List<Profile> characterStatus = new List<Profile>();

    DefaultButtonHighlight dbh;

    public void OnStatsAccessed()
    {
        CheckPlayerSelected();
        SetCharacterFoundProfiles();
        SetStatsData();
        SetRelationshipPoints();
    }

    void SetRelationshipPoints() //Set Health Bar UI based on relationship points. Red <=3, Green >=4 
    {
        //maleHealthBar.SetHealth(GameManager.ins.relationshipPoints);
    }

    void CheckPlayerSelected() //Set Data parameters based on Male or Female character selected
    {
        male.SetActive(true);

        totalEndings = totalEndings_Male;
        totalScenes = totalScenes_Male;
        profilePrefix = profilePrefix_Male;


    }


    void SetStatsData() //Stats data to be displayed on the UI
    {
        decisionsMade.text = GameManager.ins.decisionsMade.ToString();
        scenesDiscovered.text = GameManager.ins.scenesDiscovered + " / " + totalScenes;
        endingFound.text = GameManager.ins.endingsFound + " / " + totalEndings;
    }


    public void SetCharacterFoundProfiles() //Stats Screen UI for characters gettings unlocked and setting up of Alive or Dead Status
    {

        //SetDefaultProfileImages();

        //int count = GameManager.ins.characterFound.charactersDiscovered.Count;

        //List<CharacterData> temp = GameManager.ins.characterFound.charactersDiscovered;

        //for (int i = 0; i < count; i++)
        //{
        //    if (temp[i].locked)
        //        continue;

        //    if (temp[i].name.Equals(GameManager.ins.mainCharacterName))
        //    {
        //        if (GameManager.ins.relationshipPoints < 0)
        //            UnlockAntagonist();

        //        if (temp[i].type.Equals(StaticStrings.DEATH))
        //            AntagonistStatus(StaticStrings.DEATH);

        //        if (temp[i].type.Equals(StaticStrings.ALIVE))
        //            AntagonistStatus(StaticStrings.ALIVE);

        //    }
        //    else
        //    {
        //        characterStatus[i].SetCharacter(profilePrefix + temp[i].name, temp[i].type);
        //    }

        //}



    }

    void SetDefaultProfileImages()
    {
        //foreach (var item in characterStatus)
        //{
        //    item.SetDefaultSprite();
        //}
        //mainCharacter_Male.SetDefaultSprite();
        //mainCharacter_Female.SetDefaultSprite();
    }

    public void UnlockAntagonist()//Unlocks main Antagonist 
    {
        //print("Antagonist Unlocked-------------------");

        //if (GameManager.ins.relationshipPoints <= -10)
        //    GameManager.ins.relationshipPoints = 3;

        //mainCharacter_Male.SetCharacter(profilePrefix + GameManager.ins.mainCharacterName, StaticStrings.ALIVE);
        //name_Male.text = GameManager.ins.mainCharacterName;
        //maleHealthBar.SetHealth(GameManager.ins.relationshipPoints);


    }


    public void AntagonistStatus(string type)//Antogonist is Dead
    {
        //mainCharacter_Male.SetCharacter(profilePrefix + GameManager.ins.mainCharacterName, type);
        //name_Male.text = GameManager.ins.mainCharacterName;
    }


    public void BackButtonHandle()
    {
        if (dbh == null)
            dbh = GetComponent<DefaultButtonHighlight>();

        dbh.HighlightButton(dbh.defaultButton);
    }

}
