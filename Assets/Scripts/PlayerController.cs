using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D body;
    private Animator animator;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask whatIsGround;

    [SerializeField] private Vector2 wallHopDirection;
    [SerializeField] private Vector2 wallJumpDirection;

    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float jumpForce = 16.0f;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float wallSlidingSpeed;
    [SerializeField] private float movementForceInAir;
    [SerializeField] private float airDragMultiplier = 0.95f;
    [SerializeField] private float variableJumpHeightMultiplier = 0.5f;
    [SerializeField] private float wallHopForce;
    [SerializeField] private float wallJumpForce;
    private float movementInputDirection;

    private int facingDirection = 1;
    

    private bool isFacingRight = true;
    private bool isWalking;
    private bool isGrounded;
    private bool canJump;
    private bool isTouchingWall;
    private bool isWallSliding;


    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();

    }

    void FixedUpdate()
    {
        ApplyMovement();
        
        CheckSurroundings();
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();

        CheckMovementDirection();
        
        UpdateAnimations();
        
        CheckIfCanJump();

        CheckIfWallSliding();

    }

    private void CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
        if (Input.GetButtonUp("Jump"))
        {
            body.velocity = new Vector2(body.velocity.x, body.velocity.y * variableJumpHeightMultiplier);
        }
    }

    private void CheckIfCanJump()
    {
        if (isGrounded && body.velocity.y < 0.1f || isWallSliding)
        {
            canJump = true;
        }
        else
        {
            canJump = false;
        }
    }

    private void CheckIfWallSliding()
    {
        if (isTouchingWall && !isGrounded && body.velocity.y < 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void Jump()
    {
        if (canJump && !isWallSliding)
        {
            body.velocity = new Vector2(body.velocity.x, jumpForce);
            
        }
        else if (isWallSliding && movementInputDirection == 0f && canJump) //Wall hop
        {
            isWallSliding = false;
            Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -facingDirection, wallHopForce * wallHopDirection.y);
            body.AddForce(forceToAdd, ForceMode2D.Impulse);
        }
        else if((isWallSliding || isTouchingWall) && movementInputDirection != 0f && canJump)
        {
            isWallSliding = false;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            body.AddForce(forceToAdd, ForceMode2D.Impulse);
        }
    }

    private void ApplyMovement()
    {

        if (isGrounded)
        {
            body.velocity = new Vector2(speed * movementInputDirection, body.velocity.y);
        }
        else if(!isGrounded && !isWallSliding && movementInputDirection != 0)
        {
            Vector2 forceToAdd = new Vector2(movementForceInAir * movementInputDirection, 0);
            body.AddForce(forceToAdd);

            if(Mathf.Abs(body.velocity.x) > speed)
            {
                body.velocity = new Vector2(speed * movementInputDirection, body.velocity.y);
            }
        }
        else if(!isGrounded && !isWallSliding && movementInputDirection == 0f)
        {
            body.velocity = new Vector2(body.velocity.x * airDragMultiplier, body.velocity.y);
        }

        if (isWallSliding)
        {
            if (body.velocity.y < -wallSlidingSpeed)
            {
                body.velocity = new Vector2(body.velocity.x, - wallSlidingSpeed);
            }
        }
    }

    private void UpdateAnimations()
    {
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", body.velocity.y);
    }

    void CheckMovementDirection()
    {
        if (isFacingRight && movementInputDirection < 0)
        {
            Flip();
        }
        else if (!isFacingRight && movementInputDirection > 0)
        {
            Flip();
        }

        if (body.velocity.x != 0)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    void Flip()
    {
        if (!isWallSliding)
        {
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}
