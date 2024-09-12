using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IDynamicSceneObjects
{
    public static PlayerController Instance { get; private set; }
    public Enums.PlayerState currentState { get; private set; }
    Dictionary<Enums.PlayerState, PlayerBaseState> states = new()
    {
        {Enums.PlayerState.Normal, new PlayerNormalState() },
        {Enums.PlayerState.Stiff, new PlayerStiffState() }
    };
    [Header("Player Movement")]
    public float m_JumpForce = 400f;
    [Range(0, 1f)]public float m_MovementSmoothing = .05f;    // How much to smooth out the movement
    public float maxHorizontalSpeed;                          // The walk speed of the player
    public float accel;
    public float decel;
    public bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
    public LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    public LayerMask m_HarmfulGround;
    public Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    public Transform m_HeadCheck;                             // A position marking where to bounce off the ground
    public float m_GroundedRadius;                            // The distance of to check the ground
    public float m_GroundBuff;                                // The distance to jump before touching the ground
    public float m_HeadRadius;                                // The radius to bounce of the ground
    public float m_HeadBounceForce;                           // The force of the head bounce
    public int m_CoyoteTime;                                  // Coyote time 
    [Range(0, 1f)] public float lowJumpMultiplier = 2.0f;
    public float fallMultiplier = 2.5f;
    public float maxFallSpeed;
    public bool ReadyToJump { get; set; }
    public bool Bounced { get; set; }
    public bool keyJump { get; private set; }
    public bool keyJumpDown { get; private set; }
    public float keyHor { get; private set; }

    [Header("Player Rotation")]
    public float maxRotateSpeed;
    public float maxRotateAirSpeed;
    public float maxReturnSpeed;
    [Range(0, 1f)]public float returnSpeedSmoothing;
    public float rotateAccel;
    public float rotateDecel;
    public float fallingAccel;
    public float accelMultiplier;

    [Header("Arms and Legs")]
    public Collider2D arms;
    public bool ArmTouchingGround { get { return arms.IsTouchingLayers(m_HarmfulGround); } }

    [Header("Others")]
    public float inevitableSpawnTime;
    public float inevitableSpawnTimer { get; private set; }
    Vector2 checkPointPosition;
    
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Player in the scene.");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    void Start()
    {
        SwitchState(Enums.PlayerState.Normal);
        SetCheckPoint(transform.position);
    }
    void FixedUpdate()
    {
        states[currentState].FixedUpdateState(this);
        this.keyJumpDown = false;

        inevitableSpawnTimer = Mathf.Max(inevitableSpawnTimer - 1, 0);
    }
    void Update()
    {
        GetKeys();
        states[currentState].UpdateState(this);
    }
    /// <summary>
    /// Switch player to another state
    /// </summary>
    /// <param name="state"></param>
    public void SwitchState(Enums.PlayerState state)
    {
        if (states.ContainsKey(currentState))
        {
            if (states[currentState] != null)
                states[currentState].ExitState(this);
        }
        currentState = state;
        states[currentState].EnterState(this);
    }
    /// <summary>
    /// Get the input for movement
    /// </summary>
    void GetKeys()
    {
        keyHor = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump"))
        {
            keyJumpDown = true;
            keyJump = true;
        }
        else if (Input.GetButtonUp("Jump"))
        {
            keyJump = false;
        }
    }
    public PlayerBaseState GetStateInstance(Enums.PlayerState state)
    {
        return states[state];
    }
    /// <summary>
    /// Switch to stiff state, ready to respawn
    /// </summary>
    public void Die()
    {
        SwitchState(Enums.PlayerState.Stiff);
    }
    /// <summary>
    /// Set check point position of the player
    /// </summary>
    /// <param name="_checkPointPosition"></param>
    public void SetCheckPoint(Vector2 _checkPointPosition)
    {
        checkPointPosition = _checkPointPosition;
    }
    /// <summary>
    /// Set player transform to last checkpoint
    /// </summary>
    private void ReturnToCheckPoint()
    {
        transform.position = checkPointPosition;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        inevitableSpawnTimer = inevitableSpawnTime;
    }

    public void Restore()
    {
        // Return to the last checkpoint
        ReturnToCheckPoint();
        // Respawn
        SwitchState(Enums.PlayerState.Normal);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(m_GroundCheck.position, m_GroundedRadius);
        Gizmos.DrawWireSphere(m_HeadCheck.position, m_HeadRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(m_GroundCheck.position, m_GroundedRadius + m_GroundBuff);
    }
}
