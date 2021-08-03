using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameManager : MonoBehaviour
{
    public static GlobalGameManager Instance;
    public UIManager UIManager;
    public GameManager GameManager;
    
    private void Start() {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
}
