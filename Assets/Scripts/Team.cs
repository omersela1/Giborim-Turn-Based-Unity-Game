using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    #region Variables
    private Dictionary<int, CharacterClass> _currentTeam;
    public bool isInitialized;

    #endregion
    
    #region Initialization
    void Awake()
    {
        _currentTeam = new Dictionary<int, CharacterClass>();
        isInitialized = false;
    }
    
    #endregion

    #region Logic
    public void ClearCurrentTeam()
    {
        _currentTeam.Clear();
        isInitialized = false;
    }

    public void AddToCurrentTeam(CharacterClass newCharacter)
    {
        Debug.Log("Adding character to team.");
        if (_currentTeam.Count < 3)
        {
            _currentTeam.Add(_currentTeam.Count + 1, newCharacter);
        }

        if (_currentTeam.Count == 3)
            isInitialized = true;
    }

    public void RandomizeTeam()
    {
        ClearCurrentTeam();
        while (_currentTeam.Count < 3)
        {
            int randomIndex = Random.Range(0, System.Enum.GetValues(typeof(CharacterTypes)).Length);
            CharacterTypes randomType = (CharacterTypes)randomIndex;
            switch (randomType)
            {
                case CharacterTypes.Knight:
                    AddToCurrentTeam(new CharacterClass(CharacterTypes.Knight, 35, 15, 14));
                    break;
                case CharacterTypes.Pirate:
                    AddToCurrentTeam(new CharacterClass(CharacterTypes.Pirate, 40, 15, 12));
                    break;
                case CharacterTypes.Bandit:
                    AddToCurrentTeam(new CharacterClass(CharacterTypes.Bandit, 40, 15, 12));
                    break;
                case CharacterTypes.Goblin:
                    AddToCurrentTeam(new CharacterClass(CharacterTypes.Goblin, 35, 18, 10));
                    break;
                case CharacterTypes.Troll:
                    AddToCurrentTeam(new CharacterClass(CharacterTypes.Troll, 60, 20, 5));
                    break;
                case CharacterTypes.Yeti:
                    AddToCurrentTeam(new CharacterClass(CharacterTypes.Yeti, 50, 18, 8));
                    break;
            }
        }

        isInitialized = true;
    }

    public CharacterClass GetCharacterByIndex(int idx)
    {
        if (isInitialized && idx >= 1 && idx <= 3)
        {
            return _currentTeam[idx];
        }
        else
        {
            return null;
        }
        
    }

    public CharacterClass[] ExportTeamAsArray()
    {
        if (isInitialized)
        {
            CharacterClass[] export = new CharacterClass[3];
            for (int i = 1; i <= 3; i++)
            {
                export[i - 1] = GetCharacterByIndex(i);
            }
            return export;
        }
        else
        {
            Debug.Log("Team is not initialized for export.");
            return null;
        }
    }
    
    #endregion
    
}
