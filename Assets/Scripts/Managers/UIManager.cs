using System.Collections;
using System.Collections.Generic;
using MLAPI;
using UnityEngine;
using TMPro;
using MLAPI.Connection;

public class UIManager : NetworkBehaviour
{
    public GameObject optionsPanel, pausePanel, textObject, scorePanel, scoreBoard, playerScore, playerNameText;

    private List<PlayerScoreItem> playerScoreBoardItemList = new List<PlayerScoreItem>();
    private SortPlayerScoreItem sorter = new SortPlayerScoreItem();

    public void host() {
        NetworkManager.Singleton.StartHost();
        optionsPanel.SetActive(false);
        justStarted();
    }

    public void join() {
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
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void justStarted() {
        optionsPanel.SetActive(false);
        scorePanel.SetActive(true);
    }

    public void setPauseText(string text) {
        textObject.GetComponent<TextMeshProUGUI>().text = text;
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
