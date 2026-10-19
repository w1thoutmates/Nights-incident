using System.Collections;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum Speaker
{
    Player,
    Friend,
    Unknown
}

[System.Serializable]
public class DialogueData
{
    public int DialogueID;
    public Speaker Speaker;
    [TextArea(2, 5)]
    public string Text;
    public Color TextColor = Color.white;
}

public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI subText;

    [SerializeField]
    public AudioClip typingSFX;
    
    [SerializeField]
    public bool isDialogueActive;

    private Coroutine _activeDialogueRoutine;

    public void StartDialogue(DialogueData[] dialogues, float letterDelay = 0.025f)
    {
        if (_activeDialogueRoutine != null)
        {
            StopCoroutine(_activeDialogueRoutine);
            
            if (GameManager.Instance && GameManager.Instance.audioManager)
            {
                GameManager.Instance.audioManager.StopDialogueLoop();
            }
        }
        
        _activeDialogueRoutine = StartCoroutine(TypeDialogueRoutine(dialogues, letterDelay));
    }

    public IEnumerator TypeDialogueRoutine(DialogueData[] dialogues, float letterDelay = 0.025f)
    {
        isDialogueActive = true;

        foreach (DialogueData data in dialogues)
        {
            subText.text = $"<mark=#000000>{data.Text}</mark>";
            
            subText.maxVisibleCharacters = 0;
            subText.ForceMeshUpdate();
            
            int totalVisibleCharacters = subText.textInfo.characterCount;

            if (GameManager.Instance && GameManager.Instance.audioManager && typingSFX)
            {
                GameManager.Instance.audioManager.StartDialogueLoop(typingSFX);
            }

            for (int i = 0; i < totalVisibleCharacters; i++)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                {
                    subText.maxVisibleCharacters = totalVisibleCharacters;
                    yield return null;
                    break;
                }

                subText.maxVisibleCharacters = i;
                yield return new WaitForSecondsRealtime(letterDelay);
            }
            
            if (GameManager.Instance && GameManager.Instance.audioManager && typingSFX)
            {
                GameManager.Instance.audioManager.StopDialogueLoop();
            }

            while (!Input.GetMouseButton(0) || Input.GetKeyDown(KeyCode.Space))
            {
                yield return null;
            }
            
            yield return null;

            subText.text = "";
            isDialogueActive = false;
            _activeDialogueRoutine = null;
        }
    } 
}
