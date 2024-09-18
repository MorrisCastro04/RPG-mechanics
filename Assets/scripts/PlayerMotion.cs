using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMotion : MonoBehaviour
{
    public Transform camara;
    public float speed;
    public float speedRotation = 10;
    Rigidbody rb;
    Animator anim;
    Vector2 _move;
    Vector3 move;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_move.x != 0 || _move.y != 0)
        {
            move = camara.forward * _move.y;
            move += camara.right * _move.x;
            move.Normalize();
            move.y = 0;
            rb.velocity = move * speed;
            Vector3 dir = camara.forward * _move.y + camara.right * _move.x;
            dir.Normalize();
            dir.y = 0;
            // rotate to face the direction of movement
            Quaternion targetR = Quaternion.LookRotation(dir);
            Quaternion PlayerR = Quaternion.Slerp(transform.rotation, targetR, speedRotation * Time.fixedDeltaTime);
            transform.rotation = PlayerR;
        }
    }
    public void OnMove(InputValue value)
    {
        _move = value.Get<Vector2>();
        anim.SetBool("Move", (_move.x == 0 && _move.y == 0) ? false : true);
        anim.SetFloat("Moving", (_move.x == 0 && _move.y == 0) ? 0 : 1);
        if (_move.x == 0 && _move.y == 0)
            rb.velocity = Vector3.zero;
        anim.SetFloat("MoveX", _move.x);
        anim.SetFloat("MoveY", _move.y);
    }
}
