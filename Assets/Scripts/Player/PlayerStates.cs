using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState
{
    public abstract void EnterState(PlayerController player);
    public abstract void UpdateState(PlayerController player);
    public abstract void FixedUpdateState(PlayerController player);
    public abstract void ExitState(PlayerController player);
}
public class PlayerNormalState : PlayerBaseState
{
    bool m_Grounded;
    bool m_FacingRight = true;
    bool m_AirControl;
    Transform m_GroundCheck;
    float m_GroundBuff;
    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    LayerMask m_WhatIsGround;
    float maxHorizontalSpeed;
    float accelerationSpeed;
    float decelerationSpeed;
    float m_JumpForce;
    float fallMultiplier;
    float lowJumpMultiplier;

    Rigidbody2D m_Rigidbody2D;
    int _coyoteTimer = 0;
    public override void EnterState(PlayerController player)
    {
        m_Rigidbody2D = player.GetComponent<Rigidbody2D>();
        m_GroundCheck = player.m_GroundCheck;
        m_WhatIsGround = player.m_WhatIsGround;
        m_GroundBuff = player.m_GroundBuff;
        m_AirControl=  player.m_AirControl;
        maxHorizontalSpeed = player.maxHorizontalSpeed;
        accelerationSpeed = player.accel;
        decelerationSpeed = player.decel;
        m_JumpForce = player.m_JumpForce;
        fallMultiplier = player.fallMultiplier;
        lowJumpMultiplier = player.lowJumpMultiplier;

        m_Grounded = player.Grounded;
    }
    public override void UpdateState(PlayerController player)
    {
        

    }
    public override void FixedUpdateState(PlayerController player)
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;
        player.Grounded = m_Grounded;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(m_GroundCheck.position.x, m_GroundCheck.position.y - m_GroundBuff), k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].isActiveAndEnabled && colliders[i].gameObject != player.gameObject)
            {
                m_Grounded = true;
                player.Grounded = m_Grounded;
                _coyoteTimer = player.m_CoyoteTime;
            }
        }

        _coyoteTimer = Mathf.Max(_coyoteTimer - 1, 0);

        Move(player.keyHor * Time.fixedDeltaTime, player.keyJumpDown, player.keyJump, player);
    }
    void Move(float move, bool jumpDown, bool jump, PlayerController player)
    {
        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {

            // Reduce the speed by the crouchSpeed multiplier
            if ((Mathf.Abs(m_Rigidbody2D.velocity.x + _sign(move) * accelerationSpeed) <= maxHorizontalSpeed || (_sign(move) != _sign(m_Rigidbody2D.velocity.x) && Mathf.Abs(move) > 0.001f)))
            {
                m_Rigidbody2D.velocity += _sign(move) * accelerationSpeed * Vector2.right;
            }
            else if ((Mathf.Abs(m_Rigidbody2D.velocity.x + _sign(move) * accelerationSpeed) > maxHorizontalSpeed && Mathf.Abs(m_Rigidbody2D.velocity.x) < maxHorizontalSpeed))
            {
                m_Rigidbody2D.velocity = new(_sign(move) * maxHorizontalSpeed, m_Rigidbody2D.velocity.y);
            }
            if (Mathf.Abs(move) <= 0.001f || Mathf.Abs(m_Rigidbody2D.velocity.x) > maxHorizontalSpeed)
            {

                if (Mathf.Abs(m_Rigidbody2D.velocity.x) <= decelerationSpeed)
                {
                    m_Rigidbody2D.velocity *= Vector2.up;
                }
                else
                {
                    if (m_Rigidbody2D.velocity.x < 0) m_Rigidbody2D.velocity += decelerationSpeed * Vector2.right;
                    else m_Rigidbody2D.velocity -= decelerationSpeed * Vector2.right;
                }
            }
            if (player.maxFallSpeed < 0)
                m_Rigidbody2D.velocity = new(m_Rigidbody2D.velocity.x, Mathf.Max(m_Rigidbody2D.velocity.y, player.maxFallSpeed));
            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
            {
                // ... flip the player.
                Flip(player);
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight)
            {
                // ... flip the player.
                Flip(player);
            }
            // Fall faster when falling
            if (m_Rigidbody2D.velocity.y < 0)
            {
                m_Rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            // Fall faster after releasing jump key
            else if (!jump)
            {
                m_Rigidbody2D.velocity *= new Vector2(1f, lowJumpMultiplier);
                //m_Rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
            // If the player should jump
            if ((m_Grounded || _coyoteTimer > 0) && jumpDown)
            {
                
                // Add a vertical force to the player.
                m_Grounded = false;
                m_Rigidbody2D.velocity *= Vector2.right;
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            }
        }
    }
    void Flip(PlayerController player)
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = player.transform.localScale;
        theScale.x *= -1;
        player.transform.localScale = theScale;
    }
    float _sign(float x)
    {
        if (Mathf.Abs(x) < 0.0001) return 0;
        return Mathf.Sign(x);
    }
    public override void ExitState(PlayerController player)
    {

    }
}
public class PlayerStiffState : PlayerBaseState
{
    public override void EnterState(PlayerController player)
    {

    }
    public override void UpdateState(PlayerController player)
    {

    }
    public override void FixedUpdateState(PlayerController player)
    {

    }
    public override void ExitState(PlayerController player)
    {

    }
}