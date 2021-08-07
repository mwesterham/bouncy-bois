using MLAPI;
using UnityEngine;
using TMPro;

public class UIManager : NetworkBehaviour
{
    public GameObject optionsPanel, pausePanel, textObject, scorePanel, scoreText;

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

    public void setScoreText(string text) {
        scoreText.GetComponent<TextMeshProUGUI>().text = text;
    }
}
