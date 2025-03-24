using System;
using _Project.Scripts.Core.Managers;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public AirState airState;
    public IdleState idleState;
    public RunState runState;

    State state;

    // Serialized fields
    public PlayerStats stats;
    public Rigidbody2D body;
    public BoxCollider2D groundCheck;
    public LayerMask groundMask;
    public bool grounded;
    public Animator animator;
    

    // Input variables
    public InputManagerSO input {get; private set;}
    public float xInput{get; private set;}
    public bool wantsToJump{get; private set;}

    // Unity lifecycle methods
    void Start()
    {
        stats = PersistentSOManager.GetSO<PlayerStats>();
        input = PersistentSOManager.GetSO<InputManagerSO>();
        input.EnablePlayerActions();

        SetupStates();
    }

    private void SetupStates()
    {
        idleState.Setup(body, animator, this);
        airState.Setup(body, animator, this);
        runState.Setup(body, animator, this);

        state = idleState;
    }

    void Update()
    {
        CheckInput();
        HandleJumpInput();

        SelectState();
        
        state.Do();
    }

    void FixedUpdate()
    {
        CheckGround();
        HandleXMovement();
        ApplyGravity();
        ApplyFriction();
    }

    // State management
    private void SelectState()
    {
        State oldState = state;

        if (grounded)
        {
            if (xInput == 0)
            {
                state = idleState;
            }
            else
            {
                state = runState;
            }
        }
        else
        {
            state = airState;
        }

        if(oldState != state || oldState.isComplete)
        {
            oldState.Exit();
            state.Initialise();
            state.Enter();
        }
    }

    // Movement methods
    private void CheckInput()
    {
        xInput = input.Direction.x;
        wantsToJump = input.IsJumpKeyPressed;
    }

    private void HandleXMovement()
    {
        var maxXSpeed = stats.maxXSpeed;
        if (Mathf.Abs(xInput) > 0)
        {
            body.linearVelocityX = xInput * maxXSpeed;
            FaceInput();
        }
    }

    private void FaceInput()
    {
        var directionToFace = Mathf.Sign(xInput);
        transform.localScale = new Vector3(directionToFace, 1, 1);
    }

    private void HandleJumpInput()
    {
        var jumpSpeed = stats.jumpSpeed;
        if (wantsToJump && grounded)
        {
            body.linearVelocityY = jumpSpeed;
        }
    }

    // Physics methods
    private void CheckGround()
    {
        grounded = Physics2D.OverlapAreaAll(groundCheck.bounds.min, groundCheck.bounds.max, groundMask).Length > 0;
    }

    private void ApplyGravity()
    {
        var gravity = stats.gravityForce;

        if (grounded)
            body.gravityScale = 1;
        else
            body.gravityScale = gravity;
    }

    private void ApplyFriction()
    {
        var groundDecay = stats.groundDecay;

        if (grounded && xInput == 0 && body.linearVelocityY <= 0.01)
        {
            body.linearVelocityX *= groundDecay;
        }
    }
}