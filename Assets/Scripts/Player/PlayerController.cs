using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
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
    public Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    public float m_GroundedRadius;                            // The distance of to check the ground
    public float m_GroundBuff;                                // The distance to jump before touching the ground
    public int m_CoyoteTime;                                  // Coyote time 
    [Range(0, 1f)] public float lowJumpMultiplier = 2.0f;
    public float fallMultiplier = 2.5f;
    public float maxFallSpeed;
    public bool ReadyToJump { get; set; } = false;
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
    public bool ArmTouchingGround { get { return arms.IsTouchingLayers(m_WhatIsGround); } }

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
    public void Die()
    {
        SwitchState(Enums.PlayerState.Stiff);
    }
    public void SetCheckPoint(Vector2 _checkPointPosition)
    {
        checkPointPosition = _checkPointPosition;
    }
    public void ReturnToCheckPoint()
    {
        transform.position = checkPointPosition;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        inevitableSpawnTimer = inevitableSpawnTime;

        
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(m_GroundCheck.position, m_GroundedRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(m_GroundCheck.position, m_GroundedRadius + m_GroundBuff);
    }
}
