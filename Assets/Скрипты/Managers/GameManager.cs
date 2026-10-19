using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Manager References")]
    [SerializeField]
    public AudioManager audioManager;
    public ObjectiveManager objectiveManager;
    public DialogueManager dialogueManager;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // DontDestroyOnLoad(gameObject);
    }
    
    
}
