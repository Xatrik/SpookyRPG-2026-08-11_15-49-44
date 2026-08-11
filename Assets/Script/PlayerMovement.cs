using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    float playerSpeed;

    Vector2 movementVector;

    [SerializeField]
    Rigidbody2D rig;

    [SerializeField]
    Animator anim;

    [SerializeField]
    bool trigger = false;

    [SerializeField]
    bool canMoveVertically;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMoveVertically)
        {
            movementVector = new Vector2(movementVector.x, 0);
        }
        rig.linearVelocity = new Vector3(movementVector.x * playerSpeed, movementVector.y * playerSpeed, 0);

        if (movementVector.magnitude == 0)
        { 
            anim.SetInteger("Direction", 0);
        }else if (movementVector.y > 0){
            anim.SetInteger("Direction", 1);
        }else if (movementVector.y < 0){
            anim.SetInteger("Direction", 2);
        }else if (movementVector.x > 0){
            anim.SetInteger("Direction", 3);
        }else{
            anim.SetInteger("Direction", 4);
        }
    }

    void OnMove(InputValue value)
    {
        movementVector = value.Get<Vector2>();
    }

    void OnTriggerEnter2D(Collider2D other){
        if(other.gameObject.tag == "Trigger" && trigger){
            other.gameObject.GetComponent<Trigger>().triggerEvent();
            
        }
    }

    void OnTriggerExit2D(Collider2D other){
        if(other.gameObject.tag == "Trigger" && !trigger){
            trigger = true;
        }
    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        trigger = false;
    }
}
