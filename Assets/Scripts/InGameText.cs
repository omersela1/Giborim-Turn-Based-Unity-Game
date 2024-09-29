using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;

public class InGameText : MonoBehaviour
{
    #region Variables
    private TextMeshProUGUI _textObject;
    
    #endregion
    
    #region Initialization
    void OnEnable()
    {
        GameManager.CallTextChange += ChangeTextByEvent;
        GameManager.RestartTimer += ChangeTurnText;
        GameManager.ChangeCurrentTurnText += ChangeTurnText;
        Battleground.CallWinner += AnnounceWinner;
        GameLogic.OnGameRestarted += InitializeText;
        NetworkManager.OnGameRestarted += InitializeText;

        InitializeText();
    }

    void OnDisable()
    {
        GameManager.CallTextChange -= ChangeTextByEvent;
        GameManager.RestartTimer -= ChangeTurnText;
        GameManager.ChangeCurrentTurnText -= ChangeTurnText;
        Battleground.CallWinner -= AnnounceWinner;
        GameLogic.OnGameRestarted -= InitializeText;
        NetworkManager.OnGameRestarted -= InitializeText;
    }

    void InitializeText()
    {
        _textObject = gameObject.GetComponent<TextMeshProUGUI>();
        
        switch (gameObject.name)
        {
            case "CurrentTurn":
                _textObject.text = GameManager.CurrentTurn.ToString();
                break;
            case "AttackBanner":
                _textObject.text = "Fight!";
                break;
            case "Damage":
                _textObject.text = "";
                break;
        }
    }

    #endregion
    
    #region Logic
    void ChangeTextByEvent(GameSlot attacker, GameSlot defender)
    {
        CharacterClass attackingCharacter = attacker.currentCharacter;
        CharacterClass defendingCharacter = defender.currentCharacter;
        switch (gameObject.name)
        {
            case "CurrentTurn":
                _textObject.text = GameManager.CurrentTurn.ToString();
                break;
            case "AttackBanner":
                if (defendingCharacter.GetHp() > 0)
                {
                    _textObject.text = attackingCharacter.GetCharacterType().ToString() + " attacked " +
                                       defendingCharacter.GetCharacterType().ToString() + "!";
                }
                else
                {
                    _textObject.text = attackingCharacter.GetCharacterType().ToString() + " killed " +
                                       defendingCharacter.GetCharacterType().ToString() + "!";
                }
                break;
            case "Damage":
                int damage = attackingCharacter.GetAttack() - defendingCharacter.GetDefense();
                if (damage < 0)
                {
                    damage = 0;
                }
                _textObject.text = "Damage: " + damage + " HP: " + defendingCharacter.GetHp();
                break;
        }

        Thread.Sleep(200);
    }

    void ChangeTurnText()
    {
        if (gameObject.name == "CurrentTurn")
        {
            _textObject.text = GameManager.CurrentTurn.ToString();
        }
    }

    void AnnounceWinner(string winner)
    {
        switch (gameObject.name)
        {
            case "CurrentTurn":
                _textObject.text = "";
                break;
            case "AttackBanner":
                _textObject.text = winner + " wins!";
                break;
            case "Damage":
                _textObject.text = "";
                break;
        }
    }
    
    #endregion
}
