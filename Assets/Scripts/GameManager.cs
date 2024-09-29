using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using AssemblyCSharp;

#region GlobalEnums

public enum PlayMode
{
   Singleplayer,
   Multiplayer
}

public enum Turns
{
   PlayerTurn,
   EnemyTurn
}

#endregion

public class GameManager : MonoBehaviour
{
   #region Variables
   
   private static Turns _currentTurn;

   public static Turns CurrentTurn
   {
      get { return _currentTurn; }
   }

   private static PlayMode _currentPlayMode;

   public static PlayMode CurrentPlayMode
   {
      get { return _currentPlayMode; }
   }

   private float _startTime;
   
   public static event Action<GameSlot, GameSlot> CallTextChange;
   public static event Action ChangeCurrentTurnText;
   public static event Action<GameSlot, GameSlot> SendAttackToNetworkManager;
   public static event Action CallForWinCheck;
   public static event Action RestartTimer;
   
   #endregion
   
   #region Initialization
   
   private void Awake()
   {
      Listener.OnGameStarted += OnGameStarted;
      GameSlot.OnSlotsEnabled += InitializeTurn;
      GameSlot.OnSlotAttacked += HandleAttack;
      GameSlot.OnPlayerSlotFinishedMove += InitiateEnemyMove;
      Timer.TimerZero += HandleTimerZero;
      NetworkManager.OnAttackReceived += HandleAttackReceivedFromOpponent;
      NetworkManager.ResetTurns += SetTurn;
   }

   private void OnDestroy()
   {
      Listener.OnGameStarted -= OnGameStarted;
      GameSlot.OnSlotsEnabled -= InitializeTurn;
      GameSlot.OnSlotAttacked -= HandleAttack;
      GameSlot.OnPlayerSlotFinishedMove -= InitiateEnemyMove;
      Timer.TimerZero -= HandleTimerZero;
      NetworkManager.OnAttackReceived += HandleAttackReceivedFromOpponent;
      NetworkManager.ResetTurns -= SetTurn;
   }
   
   #endregion

   #region Buttons

   public void Button_BackButton()
   {
      GameLogic.Instance.Button_BackButtonLogic();
   }
   public void Button_Singleplayer()
   {
      if (GameLogic.Instance.PlayerTeam.GetComponent<Team>().isInitialized)
      {
         _currentPlayMode = PlayMode.Singleplayer;
         GameLogic.Instance.Button_SingleplayerLogic();
      }
      else
      {
         GameLogic.Instance.Button_MyTeamLogic();
      }
   }

   public void Button_Multiplayer()
   {
      if (GameLogic.Instance.PlayerTeam.GetComponent<Team>().isInitialized)
      {
         _currentPlayMode = PlayMode.Multiplayer;
         GameLogic.Instance.Button_MultiplayerLogic();
      }
      else
      {
         GameLogic.Instance.Button_MyTeamLogic();
      }
   }

   public void Button_Mltplr_Play()
   {
      GameLogic.Instance.Button_MultiplayerPlayLogic();

   }

   public void Button_MyTeam()
   {
      GameLogic.Instance.Button_MyTeamLogic();
   }

   public void Button_ApplyTeam()
   {
      GameLogic.Instance.Button_ApplyTeamLogic();
   }
   public void Button_Options()
   {
      GameLogic.Instance.Button_OptionsLogic();
   }

   public void Button_StudentInfo()
   {
      GameLogic.Instance.Button_InfoLogic();
   }

   public void Button_Restart()
   {
      GameLogic.Instance.Button_RestartLogic();
   }
   

   #endregion

   #region Events

   private void OnGameStarted(string _Sender, string _RoomId, string _NextTurn)
   {
      // this is used specifically for multiplayer mode
      string currentUserId = NetworkManager.Instance.UserId;
      if (_NextTurn == currentUserId)
      {
         _currentTurn = Turns.PlayerTurn;
      }
      else
      {
         _currentTurn = Turns.EnemyTurn;
      }
      _startTime = Time.time;
      
   }
   

   #endregion

   #region Logic
   
   private void HandleAttack(GameSlot attacker, GameSlot defender)
   {
      if (_currentPlayMode == PlayMode.Multiplayer && _currentTurn == Turns.PlayerTurn)
      {
         SendAttackToNetworkManager?.Invoke(attacker, defender);
         StartCoroutine(WaitForSendToComplete(NetworkManager.Instance.roundTripTime));
      }
      GameLogic.Instance.HandleAttackLogic(attacker, defender);
      ChangeTurn();
      CallTextChange?.Invoke(attacker, defender);
      CallForWinCheck?.Invoke();
   }

   private void HandleAttackReceivedFromOpponent(string attacker, string defender)
   {
      GameSlot attackingSlot = Battleground.Instance.GetGameSlotByName(attacker);
      GameSlot defendingSlot = Battleground.Instance.GetGameSlotByName(defender);
      if (attackingSlot != null && defendingSlot != null)
      {
         HandleAttack(attackingSlot, defendingSlot);
      }
      else
      {
         Debug.Log("Could not get slots from battleground.");
      }
   }

   private void InitializeTurn()
   {
      // initializes the first turn for singleplayer mode
      // this happens in the OnGameStarted event function for multiplayer
      if (_currentPlayMode == PlayMode.Singleplayer)
      {
         SetTurn(Turns.PlayerTurn);
      }
   }
 
   private void ChangeTurn()
   {
      if (_currentTurn == Turns.PlayerTurn)
      {
         SetTurn(Turns.EnemyTurn);
      }
      else
      {
         SetTurn(Turns.PlayerTurn);
      }
      RestartTimer?.Invoke();
   }

   public void SetTurn(Turns _newTurn)
   {
      _currentTurn = _newTurn;
      ChangeCurrentTurnText?.Invoke();
   }

   private IEnumerator AIMoveCoroutine()
   {
      yield return new WaitForSeconds(2f);
      
      GameObject[] AISlots = GameObject.FindGameObjectsWithTag("EnemySlot");
      GameObject[] PlayerSlots = GameObject.FindGameObjectsWithTag("PlayerSlot");
      if (AISlots.Length > 0 && PlayerSlots.Length > 0)
      {
         GameSlot attackingSlot = AISlots[UnityEngine.Random.Range(0, AISlots.Length)].GetComponent<GameSlot>();
         GameSlot defendingSlot = PlayerSlots[UnityEngine.Random.Range(0, PlayerSlots.Length)].GetComponent<GameSlot>();
         HandleAttack(attackingSlot, defendingSlot);
      }
   }

   private IEnumerator WaitForSendToComplete(float _roundTripTime)
   {
      yield return new WaitForSeconds(_roundTripTime);
   }

   private void InitiateAIMove()
   {
      StartCoroutine(AIMoveCoroutine());
   }
   

   private void InitiateEnemyMove()
   {
      if (_currentPlayMode == PlayMode.Singleplayer)
      {
         InitiateAIMove();
      }
   }

   private void HandleTimerZero()
   {
      ChangeTurn();
      if (_currentTurn == Turns.EnemyTurn && _currentPlayMode == PlayMode.Singleplayer)
      {
         InitiateAIMove();
      }
   }

   

   #endregion
}
