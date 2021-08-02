using MLAPI;
using UnityEngine;
using TMPro;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance;
    public GameObject optionsPanel, pausePanel, textObject;

    private void Start() {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void host() {
        NetworkManager.Singleton.StartHost();
        setOptionsPanelActive(false);
    }

    public void join() {
        NetworkManager.Singleton.StartClient();
        setOptionsPanelActive(false);
    }

    public void startServer() {
        NetworkManager.Singleton.StartServer();
        setOptionsPanelActive(false);
    }

    public void stop() {
        if(IsHost)
            NetworkManager.Singleton.StopHost();
        else if(IsClient)
            NetworkManager.Singleton.StopClient();
        else if(IsServer)
            NetworkManager.Singleton.StopServer();
        setPausePanelActive(false);
        setOptionsPanelActive(true);
    }

    public void setOptionsPanelActive(bool b) {
        optionsPanel.SetActive(b);
    }

    public void setPausePanelActive(bool b) {
        pausePanel.SetActive(b);
    }

    public void setPauseText(string text) {
        textObject.GetComponent<TextMeshProUGUI>().text = text;
    }
}
