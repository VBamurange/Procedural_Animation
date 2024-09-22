using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float legMovementSpeed = 0.2f;
    public float walkSpeed = 0.5f;
    public float rotationSpeed = 100f;
    public float bodyTurnSpeed = 50f;
    public float turnResetDelay = 1.5f;

    public Transform upperRightLeg;
    public Transform lowerRightLeg;
    public Transform midRightLeg;
    public Transform upperLeftLeg;
    public Transform lowerLeftLeg;
    public Transform midLeftLeg;

    public float minRightLegRotation;
    public float minLeftLegRotation;
    public float minMidRightLegRotation;
    public float minMidLeftLegRotation;
    public float maxRightLegRotation;
    public float maxLeftLegRotation;
    public float maxMidRightLegRotation;
    public float maxMidLeftLegRotation;

    public Transform targetPositionLeft;
    public Transform startPositionLeft;
    public Transform targetPositionRight;
    public Transform startPositionRight;
    public Transform initialRightLegPosition;

    public Transform body;
    public Transform leanForwardPosition;
    public Transform leanBackwardPosition;
    private float bodyMovementTransitionDuration = 2.0f;

    private int animationState = 0;
    private float legStepCooldown = 0.25f;
    private float switchInterval = 0.5f;
    private float timeSinceLastSwitch = 0.0f;
    private float timeSinceLastStep = 0.0f;

    public float detectionRange = 2.0f;
    public LayerMask obstacleLayer;

    private bool shouldTurnLeft = false;
    private bool canWalkStraight = true;
    private float turnCooldownTimer = 0.0f;

    private void Start()
    {
        StartCoroutine(ControlAnimationSequence());
        turnCooldownTimer = turnResetDelay;
    }

    private void Update()
    {
        if (canWalkStraight)
        {
            transform.position += transform.forward * Time.deltaTime * walkSpeed;
        }

        DetectAndAvoidObstacle();

        if (shouldTurnLeft)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 90, 0), Time.deltaTime * bodyTurnSpeed);
            Debug.Log("Turning left to avoid an obstacle.");

            if (turnCooldownTimer > turnResetDelay)
            {
                // Check again for obstacles after completing the turn
                if (!DetectAndAvoidObstacle())
                {
                    shouldTurnLeft = false;
                    canWalkStraight = true;
                    Debug.Log("Turn complete, resuming straight walk.");
                }
            }

            turnCooldownTimer += Time.deltaTime;
        }

        UpdateLegMovement();

        if (timeSinceLastSwitch > switchInterval)
        {
            animationState = 1 - animationState;
            timeSinceLastSwitch = 0;
            timeSinceLastStep = 0;
        }

        timeSinceLastSwitch += Time.deltaTime;
    }

    private IEnumerator ControlAnimationSequence()
    {
        while (true)
        {
            yield return new WaitForSeconds(6);
        }
    }

    private void UpdateLegMovement()
    {
        switch (animationState)
        {
            case 0:
                MoveLeg(upperLeftLeg, minLeftLegRotation, maxLeftLegRotation, Time.deltaTime * legMovementSpeed);
                MoveLeg(lowerLeftLeg, minMidLeftLegRotation, maxMidLeftLegRotation, Time.deltaTime * legMovementSpeed);
                ReverseMoveLeg(midRightLeg, maxRightLegRotation, minRightLegRotation, Time.deltaTime * legMovementSpeed);
                ReverseMoveLeg(lowerRightLeg, maxMidRightLegRotation, minMidRightLegRotation, Time.deltaTime * legMovementSpeed);
                ApplyBodyLeanForward(Time.deltaTime * legMovementSpeed);

                if (timeSinceLastStep > legStepCooldown)
                {
                    MoveLeg(upperRightLeg, minMidRightLegRotation, minRightLegRotation, Time.deltaTime * legMovementSpeed);
                }

                timeSinceLastStep += Time.deltaTime;
                break;

            case 1:
                ReverseMoveLeg(upperLeftLeg, maxLeftLegRotation, minLeftLegRotation, Time.deltaTime * legMovementSpeed);
                ReverseMoveLeg(lowerLeftLeg, maxMidLeftLegRotation, minMidLeftLegRotation, Time.deltaTime * legMovementSpeed);
                MoveLeg(midRightLeg, minMidRightLegRotation, maxMidRightLegRotation, Time.deltaTime * legMovementSpeed);
                MoveLeg(lowerRightLeg, minRightLegRotation, maxRightLegRotation, Time.deltaTime * legMovementSpeed);
                ApplyBodyLeanBackward(Time.deltaTime * legMovementSpeed);
                timeSinceLastStep += Time.deltaTime;
                break;
        }
    }

    private void MoveLeg(Transform leg, float startAngle, float endAngle, float deltaTime)
    {
        leg.localRotation = Quaternion.Lerp(leg.localRotation, Quaternion.Euler(endAngle, leg.localRotation.y, leg.localRotation.z), deltaTime);
    }

    private void ReverseMoveLeg(Transform leg, float startAngle, float endAngle, float deltaTime)
    {
        leg.localRotation = Quaternion.Lerp(leg.localRotation, Quaternion.Euler(startAngle, leg.localRotation.y, leg.localRotation.z), deltaTime);
    }

    private void ApplyBodyLeanForward(float deltaTime)
    {
        body.localRotation = Quaternion.Lerp(body.localRotation, leanForwardPosition.localRotation, deltaTime);
    }

    private void ApplyBodyLeanBackward(float deltaTime)
    {
        body.localRotation = Quaternion.Lerp(body.localRotation, leanBackwardPosition.localRotation, deltaTime);
    }

    private bool DetectAndAvoidObstacle()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, detectionRange, obstacleLayer))
        {
            Debug.Log("Object ahead detected, preparing to turn.");
            shouldTurnLeft = true;
            canWalkStraight = false;
            return true; // Indicates that an obstacle was detected
        }
        return false; // Indicates no obstacles detected
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("obstacle"))
        {
            Debug.Log("Collision with obstacle.");
        }
    }
}
