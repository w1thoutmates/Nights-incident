using UnityEngine;

public class R : MonoBehaviour
{
    public static R Instance;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else 
            Destroy(gameObject);
    }
    
    [Header("References")]
    
    [Header("Sounds")]
    [SerializeField]
    [Tooltip("Звук подбора предмета в руку")]
    public AudioClip takeClip;
    
    [Tooltip("Звук размещения предмета из рук")]
    [SerializeField]
    public AudioClip placeClip;
}
