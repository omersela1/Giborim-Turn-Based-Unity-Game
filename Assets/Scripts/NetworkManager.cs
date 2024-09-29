using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AssemblyCSharp;
using TMPro;
using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using com.shephertz.app42.gaming.multiplayer.client.listener;
using com.shephertz.app42.gaming.multiplayer.client.command;
using com.shephertz.app42.gaming.multiplayer.client.message;
using com.shephertz.app42.gaming.multiplayer.client.transformer;

#region SerializationWrapper

[System.Serializable]
public class SerializationWrapper
{
    public string CharacterArray;
    public string MoveData;
    
    public string TeamInitialized;

    public string sendTime;

    public SerializationWrapper(Dictionary<string, string> data)
    {
        if (data.ContainsKey("CharacterArray"))
        {
            CharacterArray = data["CharacterArray"];
            TeamInitialized = data["TeamInitialized"];
        }
        if (data.ContainsKey("MoveData"))
        {
            MoveData = data["MoveData"];
            sendTime = data["sendTime"];
        }

    }

    public Dictionary<string, string> ToDictionary()
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(CharacterArray))
        {
            dict["CharacterArray"] = CharacterArray;
        }
        if (!string.IsNullOrEmpty(TeamInitialized))
        {
            dict["TeamInitialized"] = TeamInitialized;
        }
        
        if (!string.IsNullOrEmpty(MoveData))
        {
            dict["MoveData"] = MoveData;
        }

        if (!string.IsNullOrEmpty(sendTime))
        {
            dict["sendTime"] = sendTime;
        }

        

        return dict;
    }
    
}

#endregion

#region CharacterArrayWrapper

[System.Serializable]
public class CharacterArrayWrapper
{
    public CharacterClass[] Characters;
    public CharacterArrayWrapper(CharacterClass[] toCopy)
    {
        Characters = new CharacterClass[toCopy.Length];
        for (int i = 0; i < toCopy.Length; i++)
        {
            Characters[i] = new CharacterClass(toCopy[i]);
        }
    }
}

#endregion

#region MoveData

[System.Serializable]
public class MoveData
{
    public string attackerName;
    public string defenderName;

    public MoveData(string attacker, string defender)
    {
        attackerName = attacker;
        defenderName = defender;
    }
}

#endregion
public class NetworkManager : MonoBehaviour
{

    #region Variables
    private static NetworkManager instance;

    public static NetworkManager Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            return instance;
        }
    }
    private string _apiKey = "a519f3615baf01ce6eba6c91d6c391b06014028e440ea8aaa300a1f93b91df6a";
    private string _secretKey = "ac7951ea94b0de1dd4e38029271b484ad2e34a32ceb43e1f60d4150ac7eed146";

    private Listener _listener;

    private string _userId = string.Empty;
    private string _opponentId = string.Empty;

    private string _currentRoomId = string.Empty;

    private string _currentRoomOwnerId = string.Empty;

    public string UserId
    {
        get { return _userId; }
    }

    public GameObject ConnectionStatus, PlayButton, roomNumberSlider, currentRoomId, userId, PlayerTeam, EnemyTeam, RestartRequestPanel;

    public TextMeshProUGUI roomNumberSliderText, opponentDisconnectedText;
    private Dictionary<string, object> matchRoomData;

    private List<string> roomIds;
    private int maxUsers = 2;
    private int turnTime = 11;

    private int currentRoomIndex = 0;
    
    private bool _inGame;

    public float roundTripTime;

    private float _startTime;

    public static event Action<string, string> OnAttackReceived;
    public static event Action OnGameRestarted;

    public static event Action<Turns> ResetTurns;
    
    #endregion

    #region Initialization
    private void OnEnable()
    {
        Listener.OnConnect += OnConnect;
        Listener.OnRoomsInRange += OnRoomsInRange;
        Listener.OnCreateRoom += OnCreateRoom;
        Listener.OnJoinRoom += OnJoinRoom;
        Listener.OnGetLiveRoomInfo += OnGetLiveRoomInfo;
        Listener.OnUserJoinRoom += OnUserJoinRoom;
        Listener.OnGameStarted += OnGameStarted;
        Listener.OnMoveCompleted += OnMoveCompleted;
        Listener.OnGameStopped += OnGameStopped;
        Listener.OnUserLeftRoom += OnUserLeftRoom;
        GameManager.SendAttackToNetworkManager += HandleAttackTransfer;
        Battleground.CallWinner += OnWinnerAnnounced;
        RestartTimer.RestartTimerZero += RestartGame;
    }

    private void OnDisable()
    {
        Listener.OnConnect -= OnConnect;
        Listener.OnRoomsInRange -= OnRoomsInRange;
        Listener.OnCreateRoom -= OnCreateRoom;
        Listener.OnJoinRoom -= OnJoinRoom;
        Listener.OnGetLiveRoomInfo -= OnGetLiveRoomInfo;
        Listener.OnUserJoinRoom -= OnUserJoinRoom;
        Listener.OnGameStarted -= OnGameStarted;
        Listener.OnMoveCompleted -= OnMoveCompleted;
        Listener.OnUserLeftRoom -= OnUserLeftRoom;
        GameManager.SendAttackToNetworkManager -= HandleAttackTransfer;
        Battleground.CallWinner -= OnWinnerAnnounced;
        RestartTimer.RestartTimerZero -= RestartGame;
    }
    
    void Awake()
    {
        if (_listener == null)
        {
            _listener = new Listener();
        }

        WarpClient.initialize(_apiKey, _secretKey);
        WarpClient.GetInstance().AddConnectionRequestListener(_listener);
        WarpClient.GetInstance().AddChatRequestListener(_listener);
        WarpClient.GetInstance().AddUpdateRequestListener(_listener);
        WarpClient.GetInstance().AddLobbyRequestListener(_listener);
        WarpClient.GetInstance().AddNotificationListener(_listener);
        WarpClient.GetInstance().AddRoomRequestListener(_listener);
        WarpClient.GetInstance().AddZoneRequestListener(_listener);
        WarpClient.GetInstance().AddTurnBasedRoomRequestListener(_listener);
        _userId = System.Guid.NewGuid().ToString();
        userId.GetComponent<TextMeshProUGUI>().text = "User Id: " + _userId;
        matchRoomData = new Dictionary<string, object>();

    }
    void Start()
    {
        WarpClient.GetInstance().Connect(_userId);
        PlayButton.GetComponent<Button>().interactable = false;
        SetRoomNumberSliderValue(roomNumberSlider.GetComponent<Slider>().value);
        _inGame = false;
    }
    
    #endregion
        
    #region Events
    private void UpdateStatus(string newStatus)
    {
        if (ConnectionStatus.activeSelf)
            ConnectionStatus.GetComponent<TextMeshProUGUI>().text = newStatus;
    } 

    private void OnConnect(bool _isSuccess)
    {
        if (_isSuccess)
        {
            UpdateStatus("Connected to server.");
        }

        else
        {
            UpdateStatus("Not connected to server.");
        }
        
        PlayButton.GetComponent<Button>().interactable = _isSuccess;
    }

    private void OnRoomsInRange(bool _isSuccess, MatchedRoomsEvent _eventObj)
    {
        if (_isSuccess)
        {
            UpdateStatus("Parsing rooms...");
            roomIds = new List<string>();
            foreach (var room in _eventObj.getRoomsData())
            {
                roomIds.Add(room.getId());
                Debug.Log(room.getId());
            }
            // search rooms or create one if there are no rooms
            currentRoomIndex = 0;
            RoomSearchLogic();
        }
        else
        {
            UpdateStatus("Error fetching rooms in range.");
        }
    }


    public void RequestRoomsInRange(int rangeStart, int rangeEnd)
    {
        WarpClient.GetInstance().GetRoomsInRange(rangeStart, rangeEnd);
        PlayButton.GetComponent<Button>().interactable = false;
        UpdateStatus("Searching for rooms...");
    }

    private void OnCreateRoom(bool _IsSuccess,string _RoomId)
    {
        if (_IsSuccess)
        {
            JoinRoomLogic(_RoomId, "Room created, waiting for opponent.");
        }
    }

    private void OnJoinRoom(bool _IsSuccess, string _RoomId)
    {
        if (_IsSuccess)
        {
            currentRoomId.GetComponent<TextMeshProUGUI>().text = "Room ID: " + _RoomId;
            _currentRoomId = _RoomId;
        }
        
    }

    private void OnUserJoinRoom(RoomData eventObj, string joinedUserId)
    {
        if (eventObj.getRoomOwner() == _userId && _userId != joinedUserId)
        {
            UpdateStatus("Opponent joined.");
            _opponentId = joinedUserId;
            StartCoroutine(StartGameCoroutine(2f));
        }

        _currentRoomOwnerId = eventObj.getRoomOwner();
    }

    private void OnGetLiveRoomInfo(LiveRoomInfoEvent eventObj)
    {
        if (eventObj != null && eventObj.getProperties() != null)
        {
            Dictionary<string, object> properties = eventObj.getProperties();
            if (properties.ContainsKey("Password") 
                && properties["Password"].ToString() == matchRoomData["Password"].ToString())
            {
                string roomId = eventObj.getData().getId();
                JoinRoomLogic(roomId, "Joined room: " + roomId);
            }
            else
            {
                currentRoomIndex++;
                RoomSearchLogic();
            }
        }
    }

    private void OnGameStarted(string _Sender, string _RoomId, string _NextTurn)
    {
        //prepare the host's character array to be sent to opponent side,
        //the false boolean means the host's side has not received the enemy team yet
        if (_Sender == _userId && _inGame == false)
        {
            SendTeamToOpponent(false);
        }

        _startTime = Time.time;
    }

    private void OnMoveCompleted(MoveEvent _Move)
    {
        if (_Move.getSender() != _userId && _Move.getMoveData() != null)
        {
            // receive the data
            string jsonData = _Move.getMoveData();
            Debug.Log("jsonData: " + jsonData);
            Dictionary<string, string> receivedDictionary = JsonUtility.FromJson<SerializationWrapper>(jsonData).ToDictionary();
            if (receivedDictionary.ContainsKey("CharacterArray") && receivedDictionary["CharacterArray"] != null)
            {
                // receive other player's team to assign to EnemyTeam slots
                UpdateStatus("Received wooden horse from enemy side, looks safe...");
                string characterArrayJson = receivedDictionary["CharacterArray"];
                CharacterArrayWrapper wrapper = JsonUtility.FromJson<CharacterArrayWrapper>(characterArrayJson);
                CharacterClass[] receivedCharacters = wrapper.Characters;
                
                // clear EnemyTeam on client side and assign received characters to it
                EnemyTeam.GetComponent<Team>().ClearCurrentTeam();
                foreach (CharacterClass character in receivedCharacters)
                {
                    EnemyTeam.GetComponent<Team>().AddToCurrentTeam(character);
                }
                
                if (!bool.Parse(receivedDictionary["TeamInitialized"]))
                {
                    // if we're here it means the other side has not yet received our characters
                    UpdateStatus("Sending our troops...");
                    SendTeamToOpponent(true);
                }
                
                //start the game for real
                UpdateStatus("Here we go...");
                GameLogic.Instance.GoToBattleground();
                _inGame = true;
            }
            else if (receivedDictionary.ContainsKey("MoveData"))
            {
                // this means that we're in-game, and have received a move
                string MoveDataJson = receivedDictionary["MoveData"];
                roundTripTime = Time.time - float.Parse(receivedDictionary["sendTime"]);
                Debug.Log("Received move: " + MoveDataJson);
                // this roundTripTime value will be used to delay the client-side handling of our own attack,
                // in order for the timer to be as close as possible to the opponent side's timer
                Debug.Log("RTT: " + roundTripTime);
                MoveData moveData = JsonUtility.FromJson<MoveData>(MoveDataJson);
                string attackingSlotName = TranslateSlotNameFromOpponent(moveData.attackerName);
                string defendingSlotName = TranslateSlotNameFromOpponent(moveData.defenderName);
                // let the client side scripts handle the received attack
                OnAttackReceived?.Invoke(attackingSlotName, defendingSlotName);
            }
        }
    }

    private void OnWinnerAnnounced(string _winner)
    {
        if (GameManager.CurrentPlayMode == PlayMode.Multiplayer)
        {
            StartCoroutine(StopGameCoroutine(1f));
        }
    }

    private void OnGameStopped(string _Sender, string _RoomId)
    {
        RestartRequestPanel.SetActive(true);
        RestartRequestPanel.GetComponent<Image>().sprite = SpriteBank.Instance.GetRandomSprite();
    }

    private void OnUserLeftRoom(RoomData eventObj, string username)
    {
        GameLogic.Instance.Button_BackButtonLogic();
        if (username != _userId)
        {
            StartCoroutine(OpponentDisconnectedCoroutine(3f));
            WarpClient.GetInstance().DeleteRoom(eventObj.getId());
        }
    }
    

    private void JoinRoomLogic(string newRoomId, string message)
    {
        UpdateStatus(message);
        WarpClient.GetInstance().JoinRoom(newRoomId);
        WarpClient.GetInstance().SubscribeRoom(newRoomId);
    }

    private void RoomSearchLogic()
    {
        if (currentRoomIndex < roomIds.Count)
        {
            WarpClient.GetInstance().GetLiveRoomInfo(roomIds[currentRoomIndex]);
        }
        else
        {
            UpdateStatus("Creating room...");
            WarpClient.GetInstance().CreateTurnRoom("Room " + currentRoomIndex, _userId, maxUsers, matchRoomData, turnTime);
        }
    }

    public void HandleAttackTransfer(GameSlot attackingSlot, GameSlot defendingSlot)
    {
        // prepare the data from attacking and defending slots to be transmitted
        MoveData moveToSend = new MoveData(attackingSlot.gameObject.name, defendingSlot.gameObject.name);
        string attackJson = JsonUtility.ToJson(moveToSend);
        Dictionary<string, string> data = new Dictionary<string, string>
        {
            { "MoveData", attackJson },
            { "sendTime", (Time.time - _startTime).ToString() }
        };
        string toSendJson = JsonUtility.ToJson(new SerializationWrapper(data));
        WarpClient.GetInstance().sendMove(toSendJson);
    }
    
    #endregion
    
    #region Utility

    void SendTeamToOpponent(bool _initStatus)
    {
        CharacterClass[] arrayToSend = PlayerTeam.GetComponent<Team>().ExportTeamAsArray();
        foreach (CharacterClass character in arrayToSend)
        {
            Debug.Log("Sending: " + character.GetCharacterType().ToString());
        }
        // Serialize the CharacterArrayWrapper into JSON before adding to the dictionary
        string characterArrayJson = JsonUtility.ToJson(new CharacterArrayWrapper(arrayToSend));
        Debug.Log(characterArrayJson);
        Dictionary<string, string> toSend = new Dictionary<string, string>{
            { "CharacterArray", characterArrayJson },
            { "TeamInitialized", _initStatus.ToString() }
        };

        string toSendJson = JsonUtility.ToJson(new SerializationWrapper(toSend));
        Debug.Log("toSendJson: " + toSendJson);
        // Serialize the dictionary and send the move
        WarpClient.GetInstance().sendMove(toSendJson);
        UpdateStatus("Sending team to battle...");
    }
    

    public string TranslateSlotNameFromOpponent(string slotName)
    {
        switch (slotName)
        {
            case "Slot_HostSlot1":
                return "Slot_GuestSlot1";
            case "Slot_HostSlot2":
                return "Slot_GuestSlot2";
            case "Slot_HostSlot3":
                return "Slot_GuestSlot3";
            case "Slot_GuestSlot1":
                return "Slot_HostSlot1";
            case "Slot_GuestSlot2":
                return "Slot_HostSlot2";
            case "Slot_GuestSlot3":
                return "Slot_HostSlot3";
        }
        return "";
    }

    public void SetRoomNumberSliderValue(float value)
    {
        roomNumberSliderText.text = ((int)value).ToString();
        matchRoomData["Password"] = ((int)value).ToString();
    }

    private IEnumerator StartGameCoroutine(float delayTime)
    {
        UpdateStatus("Starting game...");
        yield return new WaitForSeconds(delayTime);
        WarpClient.GetInstance().startGame();
    }

    private IEnumerator StopGameCoroutine(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        WarpClient.GetInstance().stopGame();
    }

    private IEnumerator OpponentDisconnectedCoroutine(float delayTime)
    {
        opponentDisconnectedText.text = "Opponent left the room.";
        yield return new WaitForSeconds(delayTime);
        opponentDisconnectedText.text = string.Empty;
    }

    private void RestartGame()
    {
        RestartRequestPanel.SetActive(false);
        StartCoroutine(StartGameCoroutine(1f));
        // OnGameRestarted is a different event from OnGameStarted
        // because the client side requires different actions
        OnGameRestarted?.Invoke();
        if (_currentRoomOwnerId == _userId)
        {
            ResetTurns?.Invoke(Turns.PlayerTurn);
        }
        else
        {
            ResetTurns?.Invoke(Turns.EnemyTurn);
        }
    }

    public void ExitGame()
    {
        WarpClient.GetInstance().UnsubscribeRoom(_currentRoomId);
        WarpClient.GetInstance().LeaveRoom(_currentRoomId);
        _currentRoomId = string.Empty;
        currentRoomId.GetComponent<TextMeshProUGUI>().text = "";
        _inGame = false;
        OnConnect(true);
    }
    
    
    #endregion
    
}





