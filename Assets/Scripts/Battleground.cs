using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Battleground : MonoBehaviour
{
   #region Variables

   private static Battleground instance;

   public static Battleground Instance
   {
      get
      {
         if (instance == null)
         {
            instance = GameObject.Find("Battleground").GetComponent<Battleground>();
         }
         return instance;
      }
   }

   public GameObject restartButton, restartRequestPanel;
   private GameObject[] _playerSlots, _enemySlots;
   private GameObject _timer;
   public static event Action<string> CallWinner;

   #endregion

   #region Initialization
   void Awake()
   {
      _playerSlots = GameObject.FindGameObjectsWithTag("PlayerSlot");
      _enemySlots = GameObject.FindGameObjectsWithTag("EnemySlot");
      _timer = GameObject.Find("Timer");
   }

   void OnEnable()
   {
      GameManager.CallForWinCheck += CheckWinner;
      GameLogic.OnGameRestarted += ReviveAllSlots;
      NetworkManager.OnGameRestarted += ReviveAllSlots;
      
      restartButton.SetActive(false);
      restartRequestPanel.SetActive(false);
      
      foreach (GameObject obj in _playerSlots)
      {
         obj.SetActive(true);
         obj.GetComponent<SpriteRenderer>().enabled = true;
      }
      foreach (GameObject obj in _enemySlots)
      {
         obj.SetActive(true);
         obj.GetComponent<SpriteRenderer>().enabled = true;
      }

      _timer.SetActive(true);
      
   }

   void OnDisable()
   {
      GameManager.CallForWinCheck -= CheckWinner;
      GameLogic.OnGameRestarted -= ReviveAllSlots;
      NetworkManager.OnGameRestarted -= ReviveAllSlots;

      _timer.SetActive(false);
   }
   
   #endregion
   
   #region Logic

   public void ActivateTimer()
   {
      if (!_timer.activeSelf)
      {
         _timer.SetActive(true);
      }
      _timer.GetComponent<Timer>().Restart();
   }
   void CheckWinner()
   {
      if (CheckForNoActiveSlots(_playerSlots))
      {
         CallWinner?.Invoke("Enemy");
         if (GameManager.CurrentPlayMode == PlayMode.Singleplayer)
            restartButton.SetActive(true);
      }

      if (CheckForNoActiveSlots(_enemySlots))
      {
         CallWinner?.Invoke("Player");
         if (GameManager.CurrentPlayMode == PlayMode.Singleplayer)
            restartButton.SetActive(true);
      }

      if (restartButton.activeSelf)
      {
         restartButton.GetComponent<Button>().interactable = true;
      }
      
   }

   bool CheckForNoActiveSlots(GameObject[] array)
   {
      foreach (GameObject obj in array)
      {
         if (obj.activeSelf)
         {
            return false;
         }
      }

      return true;
   }

   public GameSlot GetGameSlotByName(string name)
   {
      foreach (GameObject obj in _playerSlots)
      {
         if (obj.name == name)
            return obj.GetComponent<GameSlot>();
      }
      foreach (GameObject obj in _enemySlots)
      {
         if (obj.name == name)
            return obj.GetComponent<GameSlot>();
      }
      return null;
   }

   public void ReviveAllSlots()
   {
      foreach (GameObject obj in _playerSlots)
      {
         if (!obj.activeSelf)
         {
            obj.SetActive(true);
            obj.GetComponent<SpriteRenderer>().enabled = true;
         }
         obj.GetComponent<GameSlot>().ReinitializeSlotWithSameCharacter();
      }
      foreach (GameObject obj in _enemySlots)
      {
         if (!obj.activeSelf)
         {
            obj.SetActive(true);
            obj.GetComponent<SpriteRenderer>().enabled = true;
         }
         obj.GetComponent<GameSlot>().ReinitializeSlotWithSameCharacter();
      }

      ActivateTimer();
      if (restartButton.activeSelf)
         restartButton.SetActive(false);
   }
   
   #endregion
   
}
