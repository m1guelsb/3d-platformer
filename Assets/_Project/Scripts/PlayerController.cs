using System.Collections.Generic;
using Cinemachine;
using KBCore.Refs;
using UnityEngine;
using Utils;

namespace Platformer
{
  public class PlayerController : ValidatedMonoBehaviour
  {
    [Header("References")]
    [SerializeField, Self] Rigidbody rb;
    [SerializeField, Self] GroundChecker groundChecker;
    [SerializeField, Self] Animator animator;
    [SerializeField, Anywhere] CinemachineFreeLook freeLookVCam;
    [SerializeField, Anywhere] InputReader input;

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 15f;
    [SerializeField] float smoothTime = 0.2f;

    [Header("Jump Settings")]
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float jumpDuration = 0.5f;
    [SerializeField] float jumpCooldown = 0f;
    [SerializeField] float jumpMaxHeight = 2f;
    [SerializeField] float gravityMultiplier = 2.5f;

    private const float ZeroF = 0f;
    Transform mainCam;

    float currentSpeed;
    float velocity;
    float jumpVelocity;

    Vector3 movement;

    List<Timer> timers;
    CountdownTimer jumpTimer;
    CountdownTimer jumpCooldownTimer;


    //Animator parameters
    static readonly int Speed = Animator.StringToHash("Speed");

    void Awake()
    {
      mainCam = Camera.main.transform;
      freeLookVCam.Follow = transform;
      freeLookVCam.LookAt = transform;
      freeLookVCam.OnTargetObjectWarped(transform, transform.position - freeLookVCam.transform.position - Vector3.forward);

      rb.freezeRotation = true;

      //Setup timers
      jumpTimer = new CountdownTimer(jumpDuration);
      jumpCooldownTimer = new CountdownTimer(jumpCooldown);
      timers = new List<Timer>(2) { jumpTimer, jumpCooldownTimer };

      jumpTimer.OnTimerStop += () => jumpCooldownTimer.Start();
    }

    void Start() => input.EnablePlayerActions();

    void OnEnable()
    {
      input.Jump += OnJump;
    }

    void OnDisable()
    {
      input.Jump -= OnJump;
    }


    void OnJump(bool performed)
    {
      if (performed && !jumpTimer.IsRunning && !jumpCooldownTimer.IsRunning && groundChecker.IsGrounded)
      {
        jumpTimer.Start();
      }
      else if (!performed && jumpTimer.IsRunning)
      {
        jumpTimer.Stop();
      }
    }

    void Update()
    {
      movement = new Vector3(input.Direction.x, 0f, input.Direction.y);

      HandleTimers();
      UpdateAnimator();
    }

    void FixedUpdate()
    {
      HandleJump();
      HandleMovement();
    }

    void HandleTimers()
    {
      foreach (var timer in timers)
      {
        timer.Tick(Time.deltaTime);
      }
    }

    void HandleJump()
    {
      // If not jumping and grounded, keep jump velocity at 0
      if (!jumpTimer.IsRunning && groundChecker.IsGrounded)
      {
        jumpVelocity = ZeroF;
        jumpTimer.Stop();
        return;
      }

      // If jump or falling calculate velocity
      if (jumpTimer.IsRunning)
      {
        // Progress point for initial burst of velocity
        float launchPoint = 0.9f;
        if (jumpTimer.Progress > launchPoint)
        {
          //Calculate the velocity required to reach the jump height using physics equations v = sqrt(2 * g * h)
          jumpVelocity = Mathf.Sqrt(2 * jumpMaxHeight * Mathf.Abs(Physics.gravity.y));
        }
        else
        {
          //Gradually apply less velocity as the jump progress
          jumpVelocity += (-1 - jumpTimer.Progress) * jumpForce * Time.fixedDeltaTime;
        }
      }
      else
      {
        //Gravity takes over
        jumpVelocity += Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
      }

      //Apply velocity
      rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
    }


    void UpdateAnimator()
    {
      animator.SetFloat(Speed, currentSpeed);
    }

    private void HandleMovement()
    {

      // Rotate movement direction to match camera rotation
      var adjustDirection = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up) * movement;

      if (adjustDirection.magnitude > ZeroF)
      {
        HandleRotation(adjustDirection);
        HandleHorizontalMovement(adjustDirection);

        SmoothSpeed(adjustDirection.magnitude);
      }
      else
      {
        SmoothSpeed(ZeroF);

        //Reset horizontal velocity for a snappy stop
        rb.velocity = new Vector3(ZeroF, rb.velocity.y, ZeroF);
      }
    }

    void HandleHorizontalMovement(Vector3 adjustedDirection)
    {
      //Move the player
      Vector3 velocity = moveSpeed * Time.fixedDeltaTime * adjustedDirection;
      rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
    }

    void HandleRotation(Vector3 adjustDirection)
    {
      //Adjust rotation to match movement direction
      var targetRotation = Quaternion.LookRotation(adjustDirection);
      transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
      transform.LookAt(transform.position + adjustDirection);
    }

    void SmoothSpeed(float value)
    {
      currentSpeed = Mathf.SmoothDamp(currentSpeed, value, ref velocity, smoothTime);
    }
  }
}
