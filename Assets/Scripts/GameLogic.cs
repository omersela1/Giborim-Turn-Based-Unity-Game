using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#region GlobalEnums

public enum ScreenTypes
{
    Screen_MainMenu, 
    Screen_Singleplayer, 
    Screen_Multiplayer, 
    Screen_StudentInfo, 
    Screen_Options, 
    Screen_Loading,
    CharacterSelection,
    Battleground
}

public enum CharacterTypes
{
    Knight,
    Pirate,
    Bandit,
    Goblin,
    Troll,
    Yeti
}


#endregion

#region CharacterClass

[System.Serializable]
public class CharacterClass
{
    // these are public rather than private for serialization to json
    public CharacterTypes _type;
    public int _hp, _attack, _defense;

    public CharacterClass(CharacterTypes type, int hp, int attack, int defense)
    {
        _type = type;
        _hp = hp;
        _attack = attack;
        _defense = defense;
    }

    public CharacterClass(CharacterClass characterToCopy)
    {
        _type = characterToCopy.GetCharacterType();
        _hp = characterToCopy.GetHp();
        _attack = characterToCopy.GetAttack();
        _defense = characterToCopy.GetDefense();
    }

    public CharacterTypes GetCharacterType()
    {
        return _type;
    }

    public int GetHp()
    {
        return _hp;
    }

    public int GetAttack()
    {
        return _attack;
    }

    public int GetDefense()
    {
        return _defense;
    }

    public void SetHp(int newHp)
    {
        _hp = newHp;
    }

}

#endregion


public class GameLogic : MonoBehaviour
{
    #region ClassVariables
    
    private static GameLogic instance;
    public ScreenTypes currentScreen, prevScreen;
    public TextMeshProUGUI assignWarningText;
    private Dictionary<string, GameObject> _unityObjects;
    
    public GameObject PlayerTeam, EnemyTeam;
    
    public static GameLogic Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("GameLogic").GetComponent<GameLogic>();
            return instance;
        }
    }

    public static event Action OnGameRestarted;
    public static event Action OnEnteredMyTeamScreen;
    public static event Action OnEnteredBattlegroundScreen;
    
    #endregion

    #region Initialization
    void Awake()
    {
        _unityObjects = new Dictionary<string, GameObject>();
        GameObject[] currentObjects = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject obj in currentObjects)
        {
            _unityObjects.Add(obj.name, obj);
            Debug.Log("Added " + obj.name + " to dictionary.");
            obj.SetActive(false);
        }

        _unityObjects["Screen_MainMenu"].SetActive(true);
        currentScreen = ScreenTypes.Screen_MainMenu;
    }
    #endregion

    #region Buttons
    public void Button_BackButtonLogic()
    {
        if (GameManager.CurrentPlayMode == PlayMode.Multiplayer && currentScreen == ScreenTypes.Battleground)
        {
            NetworkManager.Instance.ExitGame();
        }
        _unityObjects[currentScreen.ToString()].SetActive(false);
        if (currentScreen == prevScreen)
            prevScreen = ScreenTypes.Screen_MainMenu;
        _unityObjects[prevScreen.ToString()].SetActive(true);
        currentScreen = prevScreen;
        AudioManager.Instance.PlayTitleTheme();
       
    }

    public void Button_SingleplayerLogic()
    {
        EnemyTeam.GetComponent<Team>().RandomizeTeam();
        GoToBattleground();
    }

    public void Button_MultiplayerLogic()
    {
        _unityObjects[currentScreen.ToString()].SetActive(false);
        prevScreen = currentScreen;
        _unityObjects["Screen_Multiplayer"].SetActive(true);
        currentScreen = ScreenTypes.Screen_Multiplayer;
        
    }

    public void Button_MultiplayerPlayLogic()
    {
        NetworkManager.Instance.RequestRoomsInRange(1, 2);
    }

    public void GoToBattleground()
    {
        _unityObjects[currentScreen.ToString()].SetActive(false);
        prevScreen = currentScreen;
        _unityObjects["Screen_Loading"].SetActive(true);
        currentScreen = ScreenTypes.Screen_Loading;
        _unityObjects["Battleground"].SetActive(true);
        // this event is fired for the ScreenSizeAdapter to adapt the background sprite size
        OnEnteredBattlegroundScreen?.Invoke();
        _unityObjects[currentScreen.ToString()].SetActive(false);
        currentScreen = ScreenTypes.Battleground;
        prevScreen = ScreenTypes.Screen_MainMenu;
        AudioManager.Instance.PlayBattleTheme();
    }

    public void Button_RestartLogic()
    {
        if (GameManager.CurrentPlayMode == PlayMode.Singleplayer)
        {
            OnGameRestarted?.Invoke();
        }
    }

    public void Button_MyTeamLogic()
    {
        _unityObjects[currentScreen.ToString()].SetActive(false);
        prevScreen = currentScreen;
        _unityObjects["CharacterSelection"].SetActive(true);
        // this event is for ScreenSizeAdapter to adapt the background image size
        OnEnteredMyTeamScreen?.Invoke();
        currentScreen = ScreenTypes.CharacterSelection;
        GameObject[] _selectionFrames = GameObject.FindGameObjectsWithTag("SelectionFrame");
        foreach (GameObject frame in _selectionFrames)
        {
            frame.SetActive(false);
        }

        assignWarningText.text = "";

    }

    public void Button_ApplyTeamLogic()
    {
        GameObject[] _selectionFrames = GameObject.FindGameObjectsWithTag("SelectionFrame");
        // check the player's selection
        if (_selectionFrames.Length < 3)
        {
            assignWarningText.text = "Must select 3 characters.";
            return;
        }
        if (_selectionFrames.Length > 3)
        {
            assignWarningText.text = "Please select only 3 characters.";
            return;
        }
        
        Team playerTeam = PlayerTeam.GetComponent<Team>();
        playerTeam.ClearCurrentTeam();
        Debug.Log("Current team cleared.");
        
        // add selected characters to player's team
        foreach (GameObject obj in _selectionFrames)
        {
            switch (obj.name)
            {
                case "SelectionFrame1":
                    playerTeam.AddToCurrentTeam(new CharacterClass(CharacterTypes.Knight, 35, 15, 14));
                    Debug.Log("Knight added to player team.");
                    break;
                case "SelectionFrame2":
                    playerTeam.AddToCurrentTeam(new CharacterClass(CharacterTypes.Pirate, 40, 15, 12));
                    Debug.Log("Pirate added to player team.");
                    break;
                case "SelectionFrame3":
                    playerTeam.AddToCurrentTeam(new CharacterClass(CharacterTypes.Bandit, 40, 15, 12));
                    Debug.Log("Bandit added to player team.");
                    break;
                case "SelectionFrame4":
                    playerTeam.AddToCurrentTeam(new CharacterClass(CharacterTypes.Goblin, 35, 18, 10));
                    Debug.Log("Goblin added to player team.");
                    break;
                case "SelectionFrame5":
                    playerTeam.AddToCurrentTeam(new CharacterClass(CharacterTypes.Troll, 60, 20, 5));
                    Debug.Log("Troll added to player team.");
                    break;
                case "SelectionFrame6":
                    playerTeam.AddToCurrentTeam(new CharacterClass(CharacterTypes.Yeti, 50, 18, 8));
                    Debug.Log("Yeti added to player team.");
                    break;
            }
        }

        Button_BackButtonLogic();

    }

    public void Button_OptionsLogic()
    {
        _unityObjects[currentScreen.ToString()].SetActive(false);
        prevScreen = currentScreen;
        _unityObjects["Screen_Options"].SetActive(true);
        currentScreen = ScreenTypes.Screen_Options;
    }

    public void Button_InfoLogic()
    {
        _unityObjects[currentScreen.ToString()].SetActive(false);
        prevScreen = currentScreen;
        _unityObjects["Screen_StudentInfo"].SetActive(true);
        currentScreen = ScreenTypes.Screen_StudentInfo;
    }
    
    #endregion

    #region Logic
    
    public void HandleAttackLogic(GameSlot attacker, GameSlot defender)
    {
        CharacterClass attackingCharacter = attacker.currentCharacter;
        CharacterClass defendingCharacter = defender.currentCharacter;
        
        int damage = attackingCharacter.GetAttack() - defendingCharacter.GetDefense();
        if (damage < 0)
        {
            damage = 0;
        }
        defendingCharacter.SetHp((defendingCharacter.GetHp() -
                                  damage));
        
        defender.HitAnimation();

        if (defendingCharacter.GetHp() <= 0)
        {
            defender.gameObject.SetActive(false);
        }
        
    }
    
    #endregion
    
}
