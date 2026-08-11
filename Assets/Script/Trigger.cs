using UnityEngine;
using UnityEngine.SceneManagement;

public class Trigger : MonoBehaviour
{
    [SerializeField]
    bool doLoadScene;
    [SerializeField]
    string eventName;
    DialogueTrigger dia;
    SpriteTrigger spri;

    [SerializeField]
    bool doDialogue;

    [SerializeField]
    bool doSprite;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (doDialogue)
        {
            dia = this.GetComponent<DialogueTrigger>();
        }
        if (doSprite)
        {
            spri = this.GetComponent<SpriteTrigger>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void triggerEvent(){
        if(doLoadScene){
            SceneManager.LoadSceneAsync(eventName);
        }

        if (dia != null)
        {
            dia.display();
        }

        if (spri != null)
        {
            spri.display();
        }

         
    }
}
