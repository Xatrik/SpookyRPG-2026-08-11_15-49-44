using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
    Dialogue dialogue;
    [SerializeField]
    string[] txt;
    [SerializeField]
    int[] speaker;
    int index = 0;
    [SerializeField]
    string nametxt;
    bool hidden = true;

    [SerializeField]
    bool destroy;

    [SerializeField] Sprite[] characterPortraits;
    [SerializeField] SpriteRenderer portrait;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogue = GameObject.Find("Dialogue").GetComponent<Dialogue>();
    }

    // Update is called once per frame
    void Update()
    {
        dialogue.hidden = hidden;
    }

    [SerializeField] private float charDelay = 0.3f; // seconds between characters
    private Coroutine running;

    public void display()
    {
        if (running != null){
            
            StopCoroutine(running);
            if (destroy)
            {
                Destroy(this.gameObject);
            }
        }
        running = StartCoroutine(DisplayRoutine());
    }

    private IEnumerator DisplayRoutine()
    {
        hidden = false;
        index = 0;
        while (index < txt.Length)
        {
            portrait.sprite = characterPortraits[speaker[index]];
            string line = txt[index];
            bool skipped = false;

            // typewriter phase — reveal one character at a time
            for (int i = 0; i < line.Length; i++)
            {
                dialogue.displayText(line.Substring(0, i + 1), nametxt);

                float t = 0f;
                while (t < charDelay)
                {
                    if (Keyboard.current.spaceKey.wasPressedThisFrame)
                    {
                        dialogue.displayText(line, nametxt); // dump full line instantly
                        //skipped = true;
                        break;
                    }
                    t += Time.deltaTime;
                    yield return null;
                }
                if (skipped) break;
            }

            // advance phase — wait for a fresh press
            yield return new WaitUntil(() => Keyboard.current.spaceKey.wasPressedThisFrame);

            // one-frame gap so the advance-press isn't reused as the next line's skip-press
            yield return null;

            index++;
        }

        hidden = true;
        
    }
}
