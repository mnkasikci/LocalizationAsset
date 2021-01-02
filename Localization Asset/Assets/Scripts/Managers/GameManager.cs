using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class GameManager : MonoBehaviour
{

    #region Singleton
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        transform.parent = null;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
    #endregion

    #region Methods
    
    void Start()
    {
        // YOU HAVE TO WAIT FOR LOCALIZATION IN SCENE MANAGER
        

    }
    

    #endregion

}

