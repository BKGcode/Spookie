using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MUS_Controller : MonoBehaviour
{
    [SerializeField]
    float speed = 4.0f;
    [SerializeField]
    float rotSpeed = 80.0f;
    float rot = 0.0f;
    [SerializeField]
    float gravity = 8.0f;

    Vector3 moveDir = Vector3.zero;
    CharacterController controller;
    Animator myAnimator;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        myAnimator = GetComponent<Animator>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.isGrounded)
        {
            if (Input.GetButton("Vertical") & Input.GetAxis("Vertical") > 0)
            {
                moveDir = new Vector3(0, 0, 1);
                moveDir *= speed;
                moveDir = transform.TransformDirection(moveDir);
                myAnimator.SetInteger("condition", 1);
                myAnimator.SetBool("isWalking", true);
            }
            else if (Input.GetButton("Vertical") & Input.GetAxis("Vertical") < 0)
            {
                moveDir = new Vector3(0, 0, -1);
                moveDir *= speed;
                moveDir = transform.TransformDirection(moveDir);
                myAnimator.SetInteger("condition", 2);
                myAnimator.SetBool("isWalking", true);
            }
            if (Input.GetButtonUp("Vertical"))
            {
                moveDir = new Vector3(0, 0, 0);
                myAnimator.SetInteger("condition", 0);
                myAnimator.SetBool("isWalking", false);
                
            }
        }
        rot += Input.GetAxis("Horizontal") * rotSpeed * Time.deltaTime;
        transform.eulerAngles = new Vector3(0, rot, 0);
        moveDir.y -= gravity * Time.deltaTime;
        controller.Move(moveDir * Time.deltaTime);
    }
}
