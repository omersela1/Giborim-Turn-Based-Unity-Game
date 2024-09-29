using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameSlot : MonoBehaviour
{
    
    #region Variables
    
    public CharacterClass currentCharacter;
    private int _initialHp;
    public GameObject PlayerTeam, EnemyTeam, SpriteBank, selectionFrame;
    public bool isClicked;
    public static event Action OnSlotsEnabled;
    public static event Action<GameSlot, GameSlot> OnSlotAttacked;
    public static event Action OnPlayerSlotFinishedMove;
    
    #endregion

    #region Initialization
    
    void OnEnable()
    {
        Team playerTeam = PlayerTeam.GetComponent<Team>();
        Team enemyTeam = EnemyTeam.GetComponent<Team>();
        SpriteBank spriteBank = SpriteBank.GetComponent<SpriteBank>();

        if (playerTeam.isInitialized && enemyTeam.isInitialized)
        {
            switch (gameObject.name)
            {
                case "Slot_HostSlot1":
                    currentCharacter = new CharacterClass(playerTeam.GetCharacterByIndex(1));
                    break;
                case "Slot_HostSlot2":
                    currentCharacter = new CharacterClass(playerTeam.GetCharacterByIndex(2));
                    break;
                case "Slot_HostSlot3":
                    currentCharacter = new CharacterClass(playerTeam.GetCharacterByIndex(3));
                    break;
                case "Slot_GuestSlot1":
                    currentCharacter = new CharacterClass(enemyTeam.GetCharacterByIndex(1));
                    break;
                case "Slot_GuestSlot2":
                    currentCharacter = new CharacterClass(enemyTeam.GetCharacterByIndex(2));
                    break;
                case "Slot_GuestSlot3":
                    currentCharacter = new CharacterClass(enemyTeam.GetCharacterByIndex(3));
                    break;
            }

            gameObject.GetComponent<SpriteRenderer>().sprite =
                spriteBank.GetSpriteByTypeName(currentCharacter.GetCharacterType().ToString());

            _initialHp = currentCharacter.GetHp();
            selectionFrame.SetActive(false);
            isClicked = false;
            OnSlotsEnabled?.Invoke();

        }
    }

    void OnDisable()
    {
        
    }
    
    #endregion
    
    #region Logic

    void OnMouseDown()
    {
        if (GameManager.CurrentTurn == Turns.PlayerTurn)
        {
            GameObject[] _teamSlots = GameObject.FindGameObjectsWithTag("PlayerSlot");
            if (gameObject.tag == "PlayerSlot")
            {
                foreach (GameObject slot in _teamSlots)
                {
                    if (slot.GetComponent<GameSlot>().isClicked)
                    {
                        slot.GetComponent<GameSlot>().isClicked = false;
                        slot.GetComponent<GameSlot>().selectionFrame.SetActive(false);
                    }
                }

                isClicked = true;
                selectionFrame.SetActive(true);
                selectionFrame.GetComponent<SpriteRenderer>().color = Color.green;
            }
            else
            {
                GameSlot _playerSelectedSlot = null;
                foreach (GameObject slot in _teamSlots)
                {
                    if (slot.GetComponent<GameSlot>().isClicked)
                        _playerSelectedSlot = slot.GetComponent<GameSlot>();
                }

                if (_playerSelectedSlot != null)
                {
                    OnSlotAttacked?.Invoke(_playerSelectedSlot, this.GetComponent<GameSlot>());
                    foreach (GameObject slot in _teamSlots)
                    {
                        slot.GetComponent<GameSlot>().isClicked = false;
                        slot.GetComponent<GameSlot>().selectionFrame.SetActive(false);
                    }
                    OnPlayerSlotFinishedMove?.Invoke();
                }
                
            }
           
        }
    }

    void OnMouseOver()
    {
        selectionFrame.SetActive(true);
        selectionFrame.GetComponent<SpriteRenderer>().color = Color.white;
    }

    void OnMouseExit()
    {
        if (!isClicked)
        {
            selectionFrame.SetActive(false);
        }
    }

    private IEnumerator DamageAnimationCoroutine(float duration, int count)
    {
        SpriteRenderer sprite = gameObject.GetComponent<SpriteRenderer>();
        for (int i = 0; i < count; i++)
        {
            sprite.enabled = false;
            yield return new WaitForSeconds(duration);
            
            sprite.enabled = true;
            yield return new WaitForSeconds(duration);
        }
        
        sprite.enabled = true;
    }

    public void HitAnimation()
    {
        StartCoroutine(DamageAnimationCoroutine(0.1f, 5));
    }
    
    // the move animation doesn't work as expected
    private IEnumerator MoveToPosition(Vector3 targetPosition, float moveSpeed)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
    }

    public void Move(Vector3 targetPosition)
    {
        StartCoroutine(MoveToPosition(targetPosition, 10f));
    }

    public void ReinitializeSlotWithSameCharacter()
    {
        currentCharacter.SetHp(_initialHp);
    }
    
    #endregion

    
}
