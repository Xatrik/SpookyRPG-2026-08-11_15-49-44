using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    [SerializeField]
    Text txt;
    [SerializeField]
    Text nametxt;
    RectTransform m_RectTransform;
    public bool hidden;
    float ypos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_RectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {

        m_RectTransform.anchoredPosition += new Vector2(0, (ypos - m_RectTransform.anchoredPosition.y) / 20);
        if (hidden)
        {
            ypos = -1000;
        } 
        else
        {
            ypos = -200;
        }
    }

    public void displayText(string text, string nametext){
        txt.text = text;
        nametxt.text = nametext;
    }
}
