using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

/// <summary>
/// Controls the player's motion, including movement, rotation, and jumping mechanics.
/// </summary>
public class PlayerMotion : MonoBehaviour
{
    /// <summary>
    /// Reference to the main camera's transform, used for directional movement.
    /// </summary>
    public Transform camara;

    /// <summary>
    /// Reference to the Cinemachine FreeLook camera, used for directional movement.
    /// </summary>
    public CinemachineFreeLook cinemachineFreeLook;

    /// <summary>
    /// Reference to the target where the camera is looking at.
    /// </summary>
    public GameObject targetCam;

    /// <summary>
    /// Speed at which the player moves.
    /// </summary>
    public float speed;

    /// <summary>
    /// Speed at which the player rotates to face the movement direction.
    /// </summary>
    public float speedRotation = 10;

    /// <summary>
    /// Vertical offset used to check if the player is grounded.
    /// </summary>
    public float groundDistanceUp;

    /// <summary>
    /// Radius used to detect the ground beneath the player.
    /// </summary>
    public float groundDistance;

    /// <summary>
    /// Force applied to the player when jumping.
    /// </summary>
    public float jumpPower = 35;

    /// <summary>
    /// Gravitational force applied to the player.
    /// </summary>
    public const float gravity = 9.8f;

    /// <summary>
    /// Multiplier used to adjust the gravitational force applied to the player.
    /// </summary>
    public float gravityMultiplier = 1;

    /// <summary>
    /// Speed at which the camera rotates around the Y-axis and X-axis.
    /// </summary>
    public float rotationSpeedCamX, rotationSpeedCamY;

    /// <summary>
    /// Boolean flag indicating whether the player is on the ground.
    /// </summary>
    public bool onGround;

    /// <summary>
    /// Boolean flag indicating whether the player is currently jumping.
    /// </summary>
    public bool isJump;

    /// <summary>
    /// Boolean flag to stop the player's movement.
    /// </summary>
    public bool stop;


    /// <summary>
    /// Layer mask used to identify ground surfaces.
    /// </summary>
    public LayerMask groundLayer;

    /// <summary>
    /// Reference to the Rigidbody component of the player for physics-based movement.
    /// </summary>
    private Rigidbody rb;

    /// <summary>
    /// Reference to the Animator component for controlling animations.
    /// </summary>
    private Animator anim;

    /// <summary>
    /// Stores the player's movement input values.
    /// </summary>
    private Vector2 _move;

    /// <summary>
    /// movment of the mpuse
    /// </summary>
    private Vector2 m_look;

    /// <summary>
    /// Stores the calculated movement direction.
    /// </summary>
    private Vector3 move;

    /// <summary>
    /// Initializes component references.
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Visualizes the ground check radius in the Unity Editor.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + (Vector3.up * groundDistanceUp), groundDistance);
    }

    /// <summary>
    /// Updates movement and rotation based on user input and ground detection.
    /// </summary>
    void FixedUpdate()
    {
        // Check if the player is touching the ground
        onGround = Physics.CheckSphere(transform.position + (Vector3.up * groundDistanceUp), groundDistance, groundLayer);

        // Apply gravity to the player
        if (!onGround)
        {
            rb.AddForce(-gravity * gravityMultiplier * Vector3.up, ForceMode.Acceleration);
        }

        //check if player touch ground after jump
        if (isJump && onGround)
        {
            isJump  = false;
            anim.SetBool("OnAir", false);
            rb.velocity = Vector3.zero;
        } else if (!isJump && !onGround) //check if player is on air without jump
        {
            anim.SetBool("OnAir", true);
            isJump = true;
            Stopping();
            anim.SetTrigger("Fall");
        }

        if (stop)
            return;

        // Handle movement input
        if (_move.x != 0 || _move.y != 0)
        {
            move = camara.forward * _move.y + camara.right * _move.x;
            move.Normalize();
            move.y = 0;
            rb.velocity = move * speed;

            // Rotate the player towards movement direction
            Vector3 dir = camara.forward * _move.y + camara.right * _move.x;
            dir.Normalize();
            dir.y = 0;
            Quaternion targetR = Quaternion.LookRotation(dir);
            Quaternion playerR = Quaternion.Slerp(transform.rotation, targetR, speedRotation * Time.fixedDeltaTime);
            transform.rotation = playerR;
        }
    }

    /// <summary>
    /// Handles movement input from the player.
    /// </summary>
    /// <param name="value">Input value representing movement direction.</param>
    public void OnMove(InputValue value)
    {
        _move = value.Get<Vector2>();

        if (stop)
            return;

        bool isMoving = _move.x != 0 || _move.y != 0;
        anim.SetBool("Move", isMoving);
        anim.SetFloat("Moving", isMoving ? 1 : 0);

        // Stop movement if no input is detected
        if (!isMoving)
            rb.velocity = Vector3.zero;

        anim.SetFloat("MoveX", _move.x);
        anim.SetFloat("MoveY", _move.y);
    }

    /// <summary>
    /// Handles jump input from the player.
    /// </summary>
    public void OnJump()
    {
        Stopping();
        isJump = true;
        anim.SetTrigger("Jumping");

        // Adjust player rotation if moving while jumping
        if (_move != Vector2.zero)
        {
            Vector3 dir = camara.forward * _move.y + camara.right * _move.x;
            dir.Normalize();
            dir.y = 0;
            Quaternion targetR = Quaternion.LookRotation(dir);
            transform.rotation = targetR;
            rb.AddForce((transform.forward + Vector3.up) * jumpPower, ForceMode.Impulse);
        }
        else // Jump straight up if not moving
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
        anim.SetBool("OnAir", true);
    }

    /// <summary>
    /// Handles camera rotation input from the player.
    /// </summary>
    /// <param name="value">Vector 2 of the delta mouse</param>
    public void OnCam(InputValue value)
    {
        m_look = value.Get<Vector2>();
        /// Rotate the camera around the player
        cinemachineFreeLook.m_XAxis.Value += m_look.x * rotationSpeedCamX;
        cinemachineFreeLook.m_YAxis.Value += m_look.y * rotationSpeedCamY * Time.fixedDeltaTime;
    }

    /// <summary>
    /// Handles the end of the fall animation.
    /// </summary>
    public void FallEnd()
    {
        StopEnd();
    }

    /// <summary>
    /// Stops movement and resets animation parameters.
    /// </summary>
    void Stopping()
    {
        if (onGround)
            rb.velocity = Vector3.zero;

        stop = true;
        anim.SetFloat("MoveX", 0);
        anim.SetFloat("MoveY", 0);
        anim.SetFloat("Moving", 0);
        anim.SetBool("Move", false);
    }

    /// <summary>
    /// restart the movement and animation parameters.
    /// </summary>
    public void StopEnd()
    {
        anim.SetBool("Move", (_move.x == 0 && _move.y == 0) ? false : true);
        anim.SetFloat("Moving", (_move.x == 0 && _move.y == 0) ? 0 : 1);
        anim.SetFloat("MoveX", _move.x);
        anim.SetFloat("MoveY", _move.y);
        rb.velocity = Vector3.zero;
        stop = false;
    }
}
