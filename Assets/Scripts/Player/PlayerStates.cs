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
    bool readyToJump;
    bool Grounded;
    bool fallingToGround;
    bool m_FacingRight = true;
    bool m_AirControl;
    Transform m_GroundCheck;
    float m_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    float m_GroundBuff;
    LayerMask m_WhatIsGround;
    float maxHorizontalSpeed;
    float accelerationSpeed;
    float decelerationSpeed;
    float m_JumpForce;
    float fallMultiplier;
    float lowJumpMultiplier;
    float maxFallSpeed;
    float rotateAccelerationSpeed;
    float rotateDecelerationSpeed;
    float fallingAccelerationSpeed;
    float maxRotateSpeed;
    float maxReturnSpeed;
    float returnSpeedSmoothing;
    float rotateSpeed;

    Rigidbody2D m_Rigidbody2D;
    int _coyoteTimer = 0;
    int _jumpingTimer = 0;
    public override void EnterState(PlayerController player)
    {
        m_Rigidbody2D = player.GetComponent<Rigidbody2D>();
        m_GroundCheck = player.m_GroundCheck;
        m_WhatIsGround = player.m_WhatIsGround;
        m_GroundedRadius = player.m_GroundedRadius;
        m_GroundBuff = player.m_GroundBuff;
        m_AirControl =  player.m_AirControl;
        maxHorizontalSpeed = player.maxHorizontalSpeed;
        accelerationSpeed = player.accel;
        decelerationSpeed = player.decel;
        m_JumpForce = player.m_JumpForce;
        fallMultiplier = player.fallMultiplier;
        lowJumpMultiplier = player.lowJumpMultiplier;
        maxFallSpeed = player.maxFallSpeed;
        rotateAccelerationSpeed = player.rotateAccel;
        rotateDecelerationSpeed = player.rotateDecel;
        fallingAccelerationSpeed = player.fallingAccel;
        maxRotateSpeed = player.maxRotateSpeed;
        maxReturnSpeed = player.maxReturnSpeed;
        returnSpeedSmoothing = player.returnSpeedSmoothing;

        readyToJump = player.ReadyToJump;
    }
    public override void UpdateState(PlayerController player)
    {
        if (player.keyJumpDown)
        {
            _jumpingTimer = player.m_CoyoteTime;
        }
    }
    public override void FixedUpdateState(PlayerController player)
    {
        bool wasGrounded = readyToJump;
        readyToJump = false;
        fallingToGround = false;
        player.ReadyToJump = readyToJump;

        // The player is ready to jump if a circlecast to the groundcheck position hits anything designated as ground
        Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(m_GroundCheck.position.x, m_GroundCheck.position.y), m_GroundedRadius + m_GroundBuff, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].isActiveAndEnabled && colliders[i].gameObject != player.gameObject)
            {
                readyToJump = true;
                player.ReadyToJump = readyToJump;
                _coyoteTimer = player.m_CoyoteTime;
            }
        }

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        colliders = Physics2D.OverlapCircleAll(new Vector2(m_GroundCheck.position.x, m_GroundCheck.position.y), m_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].isActiveAndEnabled && colliders[i].gameObject != player.gameObject)
            {
                fallingToGround = true;
            }
        }

        _coyoteTimer = Mathf.Max(_coyoteTimer - 1, 0);
        _jumpingTimer = Mathf.Max(_jumpingTimer - 1, 0);

        Move(player.keyHor * Time.fixedDeltaTime, _jumpingTimer > 0, player.keyJump, player);
    }
    void Move(float move, bool jumpDown, bool jump, PlayerController player)
    {
        // only control the player if grounded or airControl is turned on
        if (readyToJump || m_AirControl)
        {
            #region Player Horizontal Movement
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
            #endregion

            #region Rotation
            if ((Mathf.Abs(rotateSpeed + _sign(move) * rotateAccelerationSpeed) <= maxRotateSpeed || (_sign(move) != _sign(rotateSpeed) && Mathf.Abs(move) > 0.001f)))
            {
                rotateSpeed += _sign(move) * rotateAccelerationSpeed;
            }
            else if ((Mathf.Abs(rotateSpeed + _sign(move) * rotateAccelerationSpeed) > maxRotateSpeed && Mathf.Abs(rotateSpeed) < maxRotateSpeed))
            {
                rotateSpeed = _sign(move) * maxRotateSpeed;
            }
            rotateSpeed = Mathf.Clamp(rotateSpeed, -maxRotateSpeed, maxRotateSpeed);
            if(Mathf.Abs(move) <= 0.001f && m_Rigidbody2D.velocity.y > 0 && jump) // Player not moving, Holding space and rising
            {
                // Start Turn Back
                var _playerAngleDiff = Mathf.DeltaAngle(player.transform.rotation.z * Mathf.Rad2Deg, 0f);
                rotateSpeed = _playerAngleDiff * (1f - returnSpeedSmoothing);

                /*if (rotateSpeed + rotateAccelerationSpeed > Mathf.Abs(_playerAngleDiff * (1f - returnSpeedSmoothing)))
                {
                    rotateSpeed = _playerAngleDiff * (1f - returnSpeedSmoothing);
                }
                else
                {
                    rotateSpeed += rotateAccelerationSpeed * _sign(_playerAngleDiff);
                }*/
                rotateSpeed = Mathf.Clamp(rotateSpeed, -maxReturnSpeed, maxReturnSpeed);
            }
            else if(Mathf.Abs(move) <= 0.001f && fallingToGround && !player.ArmTouchingGround)
            {
                var _playerAngleDiff = Mathf.DeltaAngle(player.transform.rotation.z * Mathf.Rad2Deg, 0f);
                rotateSpeed += _reverseSign(_playerAngleDiff) * fallingAccelerationSpeed;
            }
            else if ((Mathf.Abs(move) <= 0.001f && m_Rigidbody2D.velocity.y > 0 && !fallingToGround) || Mathf.Abs(rotateSpeed) > maxRotateSpeed || player.ArmTouchingGround) // Stop Rotation
            {
                if (Mathf.Abs(rotateSpeed) <= rotateDecelerationSpeed)
                {
                    rotateSpeed = 0f;
                }
                else
                {
                    if (rotateSpeed < 0) rotateSpeed += rotateDecelerationSpeed;
                    else rotateSpeed -= rotateDecelerationSpeed;
                }
            }
            player.transform.Rotate(0f, 0f, rotateSpeed);
            #endregion
            if (maxFallSpeed < 0)
                m_Rigidbody2D.velocity = new(m_Rigidbody2D.velocity.x, Mathf.Max(m_Rigidbody2D.velocity.y, maxFallSpeed));
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
        }
        // If the player should jump
        if ((readyToJump || _coyoteTimer > 0) && jumpDown)
        {
            // Reset the jumping timer
            _jumpingTimer = 0;
            // Add a vertical force to the player.
            readyToJump = false;
            m_Rigidbody2D.velocity *= Vector2.right;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
        }
    }
    /// <summary>
    /// Flips the player horizontally
    /// </summary>
    /// <param name="player"></param>
    void Flip(PlayerController player)
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = player.transform.localScale;
        theScale.x *= -1;
        player.transform.localScale = theScale;
    }
    /// <summary>
    /// Returns the sign of the number including zero
    /// </summary>
    /// <param name="x"></param>
    /// <returns>The sign of the number</returns>
    float _sign(float x)
    {
        if (Mathf.Abs(x) < 0.001) return 0;
        return Mathf.Sign(x);
    }
    /// <summary>
    /// Get the sign of the number and reverse it, including zero
    /// </summary>
    /// <param name="x"></param>
    /// <returns>The number's reversed sign</returns>
    float _reverseSign(float x)
    {
        return _sign(x) * -1;
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