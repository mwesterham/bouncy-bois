using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine;
using TMPro;
using MLAPI.Connection;
using UnityEngine.SceneManagement;

public class UIManager : NetworkBehaviour
{
    public GameObject startMenuPanel,
    optionsPanel, 
    pausePanel, 
    pauseTextObject, 
    scorePanel, 
    scoreBoard,
    playerNameText,
    playerScore,
    joinOptions,
    hostButton, joinButton, ipInput;
    public TMP_InputField inputField;

    private List<PlayerScoreItem> playerScoreBoardItemList = new List<PlayerScoreItem>();
    private SortPlayerScoreItem sorter = new SortPlayerScoreItem();

    public void gotoOptions(bool isHosting) {
        optionsPanel.SetActive(false);
        joinOptions.SetActive(true);
        if(isHosting) {
            joinButton.SetActive(false);
            ipInput.SetActive(false);
        }
        else
            hostButton.SetActive(false);
        
    }
    
    public void host() {
        NetworkManager.Singleton.StartHost();
        justStarted();
    }

    public void join() {
        string ip = inputField.text;
        if(ip.Length <= 1) {
            NetworkManager.Singleton.GetComponentInChildren<UNetTransport>().ConnectAddress = "127.0.0.1";
        }
        else {
            Debug.Log("Connecting to " + ip);
            NetworkManager.Singleton.GetComponentInChildren<UNetTransport>().ConnectAddress = ip;
        }
        
        NetworkManager.Singleton.StartClient();
        justStarted();
    }

    public void startServer() {
        NetworkManager.Singleton.StartServer();
        justStarted();
    }

    public void stop() {
        if(IsHost)
            NetworkManager.Singleton.StopHost();
        else if(IsClient)
            NetworkManager.Singleton.StopClient();
        else if(IsServer)
            NetworkManager.Singleton.StopServer();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void backToChoice() {
        joinButton.SetActive(true);
        ipInput.SetActive(true);
        hostButton.SetActive(true);
        optionsPanel.SetActive(true);
        joinOptions.SetActive(false);
    }

    public void justStarted() {
        joinOptions.SetActive(false);
        startMenuPanel.SetActive(false);
        scorePanel.SetActive(true);
    }

    public void setPauseText(string text) {
        pauseTextObject.GetComponent<TextMeshProUGUI>().text = text;
    }

    public string getEnteredName() {
        return playerNameText.GetComponent<TextMeshProUGUI>().text;
    }

    public void addPlayerScore(ulong id, string name, float score) {
        playerScoreBoardItemList.Add(new PlayerScoreItem(id, name, score));
        updateScoreBoard();
    }

    public void removePlayerScore(ulong id) {
        foreach(PlayerScoreItem p in playerScoreBoardItemList) {
            if(p.id == id) {
                playerScoreBoardItemList.Remove(p);
                break;
            }
        }

        updateScoreBoard();
    }

    public void removeAllPlayerScores() {
        playerScoreBoardItemList.Clear();
        updateScoreBoard();
    }

    private void updateScoreBoard() {
        foreach(Transform scoreText in scoreBoard.transform) {
            Destroy(scoreText.gameObject);
        }

        playerScoreBoardItemList.Sort(sorter);

        foreach (PlayerScoreItem p in playerScoreBoardItemList) {
            GameObject playerScoreInstance = Instantiate(playerScore, scoreBoard.transform);
            playerScoreInstance.GetComponent<TextMeshProUGUI>().text = p.name + " - " + p.score;
        }
    }

    private class PlayerScoreItem {
        public ulong id;
        public string name;
        public float score;

        public PlayerScoreItem(ulong id, string name, float score) {
            this.id = id;
            this.name = name;
            this.score = score;
        }
    }

    private class SortPlayerScoreItem : IComparer<PlayerScoreItem> {
        public int Compare(PlayerScoreItem x, PlayerScoreItem y) {
            if(x.score <= y.score) {
                return 1;
            }
            else if (x.score > y.score) {
                return -1;
            }
            else return 0;
        }
    }
}
