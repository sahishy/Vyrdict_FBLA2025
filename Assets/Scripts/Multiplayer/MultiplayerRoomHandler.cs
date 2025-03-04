using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class MultiplayerRoomHandler : MonoBehaviourPunCallbacks
{
    public static MultiplayerRoomHandler instance;

    public string username;
    public string roomCode;

    [Header("References")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TMP_Text generatedRoomCodeText;

    void Awake() {
        if(instance == null) {
            instance = this;
        } else { 
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        PhotonNetwork.ConnectUsingSettings();
        GenerateRandomName();
    }

    public void CreateRoom()
    {
        roomCode = GenerateRoomCode();
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };

        PhotonNetwork.CreateRoom(roomCode, roomOptions);

        generatedRoomCodeText.text = $"Code: {roomCode}";
    }

    public void JoinRoom()
    {
        string enteredCode = roomCodeInput.text;
        if(!string.IsNullOrEmpty(enteredCode)) {
            PhotonNetwork.JoinRoom(enteredCode);
        } else {
            Debug.Log("no code entered");
        }
    }

    private string GenerateRoomCode() {
        return Random.Range(100000, 999999).ToString();
    }

    public override void OnJoinedRoom() {
        Debug.Log($"joined room: {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message) {
        Debug.LogError($"room creation failed: {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message) {
        Debug.LogError($"failed to join room: {message}");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) {
        if(PhotonNetwork.IsMasterClient) {
            Debug.Log($"Player {newPlayer.NickName} joined the room!");
            photonView.RPC("NotifyRoomOwner", RpcTarget.MasterClient, newPlayer.NickName);

            if(PhotonNetwork.CurrentRoom.PlayerCount == 2) {
                PhotonNetwork.LoadLevel("MainScene");
            }
        }
    }

    [PunRPC]
    void NotifyRoomOwner(string playerName)
    {
        Debug.Log($"New Player Joined: {playerName}");
        // You can now trigger a UI update or a notification for the host
    }

    public void GenerateRandomName() {
        string randomName = $"User{Random.Range(0, 9999)}";
        username = randomName;
        usernameInput.text = username;

        PhotonNetwork.NickName = username;
    }

    public void ChangeName() {
        username = usernameInput.text;

        PhotonNetwork.NickName = username;
    }
}