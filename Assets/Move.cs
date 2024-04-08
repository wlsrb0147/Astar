using UnityEngine;
using UnityEngine.InputSystem;


public class Move : MonoBehaviour
{
    private Vector3 dir;
   // private CharacterController _controller;
    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
     //   _controller = gameObject.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    { 
        rb.velocity =  dir ;
   //     rb.MovePosition(transform.position + dir * Time.deltaTime);
   //   _controller.Move(dir * Time.deltaTime);
    }

    public void OnMove(InputValue value)
    {
        dir = value.Get<Vector2>();
        Debug.Log(dir);
    }
}
