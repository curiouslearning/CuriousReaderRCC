using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GGameManagerInitializer : MonoBehaviour {

    public GGameManager gameManager;

    public bool makePersistent = true;

    void Awake()
    {
        if (!GGameManager.Instance)
        {
            Debug.Log("NO GAME MANAGER INSTANCE FOUND - CREATING ONE!");
           // GGameManager instance = Instantiate(gameManager) as GGameManager;

            //if (makePersistent)
              //  DontDestroyOnLoad(instance.gameObject);
        }

       //Destroy(this.gameObject);
    }
   
}
