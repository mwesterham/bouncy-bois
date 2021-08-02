using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameManager : MonoBehaviour
{
    public static GlobalGameManager Instance;
    public UIManager uIManager;
    
    private void Start() {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
}
