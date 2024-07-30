using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    public Enums.PlayerState currentState { get; set; }
    Dictionary<Enums.PlayerState, PlayerBaseState> states = new Dictionary<Enums.PlayerState, PlayerBaseState>
    {
        {Enums.PlayerState.Normal, new PlayerNormalState() },
        {Enums.PlayerState.Stiff, new PlayerStiffState() }
    };
    [Header("Player Movement")]
    public Transform forcePoint;
    public float m_JumpForce = 400f;
    [Range(0, 1f)]public float m_MovementSmoothing = .05f;    // How much to smooth out the movement
    public float maxHorizontalSpeed;                          // The walk speed of the player
    public float accel;
    public float decel;
    public bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
    public LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    public Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    public float m_GroundBuff;                                // Distance above ground is alloed to jump
    public int m_CoyoteTime;                                  // Coyote time 
    [Range(0, 1f)] public float lowJumpMultiplier = 2.0f;
    public float fallMultiplier = 2.5f;
    public float maxFallSpeed;
    public bool Grounded { get; set; } = false;
    public bool keyJump { get; private set; }
    public bool keyJumpDown { get; private set; }
    public float keyHor { get; private set; }
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            Debug.LogError("Found more than one Player in the scene.");
        }
        else
        {
            Instance = this;
        }
    }
    void Start()
    {
        SwitchState(Enums.PlayerState.Normal);
    }
    void FixedUpdate()
    {
        states[currentState].FixedUpdateState(this);
        this.keyJumpDown = false;
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
}
