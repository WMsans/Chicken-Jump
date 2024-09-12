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
    private bool readyToJump;
    private bool Grounded;
    private bool fallingToGround;
    private bool m_FacingRight = true;
    private bool m_AirControl;
    private Transform m_GroundCheck;
    private Transform m_HeadCheck;
    private float m_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    private float m_GroundBuff;
    private float m_HeadRadius;
    private float m_HeadBounceForce;
    private LayerMask m_WhatIsGround;
    private float maxHorizontalSpeed;
    private float accelerationSpeed;
    private float decelerationSpeed;
    private float m_JumpForce;
    private float fallMultiplier;
    private float lowJumpMultiplier;
    private float maxFallSpeed;
    private float rotateAccelerationSpeed;
    private float rotateDecelerationSpeed;
    private float fallingAccelerationSpeed;
    private float maxRotateSpeed;
    private float maxRotateAirSpeed;
    private float maxReturnSpeed;
    private float returnSpeedSmoothing;
    private float rotateSpeed;
    private float accelMultiplier;
    private bool accelingRotate = false;
    private float wasMoving;
    private float bounceTimer;

    private Rigidbody2D m_Rigidbody2D;
    private int coyoteTimer = 0;
    private int jumpingTimer = 0;
    public override void EnterState(PlayerController player)
    {
        m_Rigidbody2D = player.GetComponent<Rigidbody2D>();
        m_GroundCheck = player.m_GroundCheck;
        m_HeadCheck = player.m_HeadCheck;
        m_WhatIsGround = player.m_WhatIsGround;
        m_GroundedRadius = player.m_GroundedRadius;
        m_GroundBuff = player.m_GroundBuff;
        m_HeadRadius = player.m_HeadRadius;
        m_HeadBounceForce = player.m_HeadBounceForce;
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
        maxRotateAirSpeed = player.maxRotateAirSpeed;
        maxReturnSpeed = player.maxReturnSpeed;
        returnSpeedSmoothing = player.returnSpeedSmoothing;
        accelMultiplier = player.accelMultiplier;
        accelingRotate = false;
        wasMoving = 0;

        rotateSpeed = 0;
        readyToJump = player.ReadyToJump;
    }
    public override void UpdateState(PlayerController player)
    {
        if (player.keyJumpDown)
        {
            jumpingTimer = player.m_CoyoteTime;
        }
        if (player.ArmTouchingGround)
        {
            Die(player);
        }
    }

    private void Die(PlayerController player)
    {
        if(player.inevitableSpawnTimer <= 0)
        {
            // Stiff state
            player.SwitchState(Enums.PlayerState.Stiff);
        }
        
    }
    public override void FixedUpdateState(PlayerController player)
    {
        bool wasGrounded = readyToJump;
        readyToJump = false;
        fallingToGround = false;
        player.ReadyToJump = readyToJump;

        // The player is ready to jump if a circlecast to the groundcheck position hits anything designated as ground
        var colliders = new Collider2D[12];
        colliders = Physics2D.OverlapCircleAll(new Vector2(m_GroundCheck.position.x, m_GroundCheck.position.y), m_GroundedRadius + m_GroundBuff, m_WhatIsGround);
        foreach (var t in colliders)
        {
            if (t.isActiveAndEnabled && t.gameObject != player.gameObject)
            {
                readyToJump = true;
                player.ReadyToJump = readyToJump;
                player.Bounced = false;
                coyoteTimer = player.m_CoyoteTime;
            }
        }

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        colliders = Physics2D.OverlapCircleAll(new Vector2(m_GroundCheck.position.x, m_GroundCheck.position.y), m_GroundedRadius, m_WhatIsGround);
        foreach (var t in colliders)
        {
            if (t.isActiveAndEnabled && t.gameObject != player.gameObject)
            {
                fallingToGround = true;
            }
        }
        // The player should be bounce when head is touching ground
        colliders = Physics2D.OverlapCircleAll(new Vector2(m_HeadCheck.position.x, m_HeadCheck.position.y), m_HeadRadius, m_WhatIsGround);
        foreach (var t in colliders)
        {
            if (t.isActiveAndEnabled && t.gameObject != player.gameObject)
            {
                Bounce(player, t);
            }
        }
        coyoteTimer = Mathf.Max(coyoteTimer - 1, 0);
        jumpingTimer = Mathf.Max(jumpingTimer - 1, 0);
        bounceTimer = Mathf.Max(bounceTimer - 1, 0);

        Move(player.keyHor * Time.fixedDeltaTime, jumpingTimer > 0, player.keyJump, player);
    }

    private void Bounce(PlayerController player, Collider2D groundCollider)
    {
        // Check if the player is downward and not bounced
        if (Mathf.Abs(Mathf.DeltaAngle(m_Rigidbody2D.rotation, 0f)) > 120f && m_HeadCheck.position.y > groundCollider.bounds.min.y && bounceTimer <= 0f)
        {
            player.Bounced = true;
            m_Rigidbody2D.velocity *= Vector2.right;
            m_Rigidbody2D.AddForce(new Vector2(0, m_HeadBounceForce), ForceMode2D.Impulse);
            bounceTimer = player.m_CoyoteTime;
        }
    }

    private void Move(float move, bool jumpDown, bool jump, PlayerController player)
    {
        // only control the player if grounded or airControl is turned on
        if (readyToJump || m_AirControl)
        {
            #region Player Horizontal Movement
            if(_sign(move) != 0 && wasMoving != _sign(move))
            {
                // multiply rotation
                accelingRotate = true;
            }
            wasMoving = _sign(move);
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
            var _currentMaxRotateSpeed = maxRotateSpeed;
            if (!fallingToGround) _currentMaxRotateSpeed = maxRotateAirSpeed;
            
            if(accelingRotate) 
            { 
                rotateSpeed += accelMultiplier; 
                accelingRotate = false; 
            }
            if ((Mathf.Abs(rotateSpeed + _sign(move) * rotateAccelerationSpeed) <= _currentMaxRotateSpeed || (_sign(move) != _sign(rotateSpeed) && Mathf.Abs(move) > 0.001f)))
            {
                rotateSpeed += _sign(move) * rotateAccelerationSpeed;
            }
            else if ((Mathf.Abs(rotateSpeed + _sign(move) * rotateAccelerationSpeed) > _currentMaxRotateSpeed && Mathf.Abs(rotateSpeed) < _currentMaxRotateSpeed))
            {
                rotateSpeed = _sign(move) * _currentMaxRotateSpeed;
            }
            //rotateSpeed = Mathf.Clamp(rotateSpeed, -_currentMaxRotateSpeed, _currentMaxRotateSpeed);
            if(Mathf.Abs(move) <= 0.001f && m_Rigidbody2D.velocity.y > 0 && jump) // Player not moving, Holding space and rising
            {
                // Start Turn Back
                var _playerAngleDiff = Mathf.DeltaAngle(player.transform.rotation.z * Mathf.Rad2Deg, 0f);
                rotateSpeed = _playerAngleDiff * (1f - returnSpeedSmoothing);
                rotateSpeed = Mathf.Clamp(rotateSpeed, -maxReturnSpeed, maxReturnSpeed);
            }
            else if(Mathf.Abs(move) <= 0.001f && fallingToGround && !player.ArmTouchingGround)
            {
                var _playerAngleDiff = Mathf.DeltaAngle(player.transform.rotation.z * Mathf.Rad2Deg, 0f);
                rotateSpeed += -Mathf.Sign(_playerAngleDiff) * fallingAccelerationSpeed;
            }
            else if ((Mathf.Abs(move) <= 0.001f && m_Rigidbody2D.velocity.y > 0 && !fallingToGround) || Mathf.Abs(rotateSpeed) > _currentMaxRotateSpeed || player.ArmTouchingGround) // Stop Rotation
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
            if (player.arms.IsTouchingLayers(player.m_WhatIsGround)) rotateSpeed = 0;
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
        // Variable jump
        if(!player.Bounced)
            // Fall faster when falling
            if (m_Rigidbody2D.velocity.y < 0)
            {
                m_Rigidbody2D.velocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime);
            }
            // Fall faster after releasing jump key
            else if (!jump)
            {
                m_Rigidbody2D.velocity *= new Vector2(1f, lowJumpMultiplier);
            }
        // If the player should jump
        if ((readyToJump || coyoteTimer > 0) && jumpDown && player.inevitableSpawnTimer <= 0)
        {
            // Reset the jumping timer
            jumpingTimer = 0;
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
    private void Flip(PlayerController player)
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
    private float _sign(float x)
    {
        if (Mathf.Abs(x) < 0.001) return 0;
        return Mathf.Sign(x);
    }
    /// <summary>
    /// Get the sign of the number and reverse it, including zero
    /// </summary>
    /// <param name="x"></param>
    /// <returns>The number's reversed sign</returns>
    private float _reverseSign(float x)
    {
        return _sign(x) * -1;
    }
    public override void ExitState(PlayerController player)
    {

    }
}
public class PlayerStiffState : PlayerBaseState
{
    private Rigidbody2D rd;
    public override void EnterState(PlayerController player)
    {
        rd = player.GetComponent<Rigidbody2D>();
        player.GetComponent<SpriteRenderer>().color = Color.red;

        // Get all objects that touchs arms
        var cols = new Collider2D[12];
        var colCount = player.arms.GetContacts(cols);
        if (colCount > 0)
            for (var i = 0; i < colCount; i++)
            {
                var col = cols[i];
                if(col == null) continue;
                if (col.CompareTag("HiddenBlock"))
                {
                    // If the block is hidden, make it shown
                    col.GetComponent<HiddenBlock>().ShowBlock();
                }
            }
    }
    public override void UpdateState(PlayerController player)
    {
        if (Input.GetButtonDown("Jump"))
        {
            DynamicManager.Instance.RestartGame();
        }
    }
    public override void FixedUpdateState(PlayerController player)
    {
        rd.velocity = Vector2.zero;
        rd.gravityScale = 0;
    }
    public override void ExitState(PlayerController player)
    {
        player.GetComponent<SpriteRenderer>().color = Color.white;
        rd.gravityScale = 3;
    }
}