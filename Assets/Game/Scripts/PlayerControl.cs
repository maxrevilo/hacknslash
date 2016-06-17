using System;
using UnityEngine;

// [RequireComponent(typeof(PlayerMain))]
[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(PlayerMotion))]
public class PlayerControl : MonoBehaviour {
	
    // private PlayerMain playerMain;
    private PlayerAttack playerAttack;
    private PlayerMotion playerMotion;

	private Camera mainCamera;
    
    [SerializeField]
    private float distanceToStopOnTap = 0.5f;
    [SerializeField]
    private float distanceToAttackOnTap = 5.5f;

    [SerializeField]
    private float axisVectorDeadZone = 0.1f;
    [SerializeField]
    private float timeToEnterInHolding = 0.3f;
    [SerializeField]
    private float distanceToConsiderDash = 0.3f;
    [SerializeField]
    private float tapRayRadius = 0.7f;

    private float pressBegin = -1;
    private Vector3 pressPosition;
    private Vector3 lastDashWorldVector;

    private enum ControlState
    { Idle, Moving, Attacking }

    private ControlState controlState = ControlState.Idle;
    private Vector3 attackMark;

    private enum TouchType
    { None, Tap, Holding, HoldRelase, Dash, HeldDash }
    
    private int playersLayerMask;

    void Awake() {
		// playerMain = GetComponent<PlayerMain>();
		playerAttack = GetComponent<PlayerAttack>();
		playerMotion = GetComponent<PlayerMotion>();
		mainCamera = Camera.main;
        playersLayerMask = LayerMask.GetMask(new String[]{"Player"});
	}

	void Start () {
        
    }

    void Update()
    {
        switch (CheckTouchType())
        {
            case TouchType.Tap:
                SetAttackOrMoveMode();
                break;
            case TouchType.Holding:
                ChargeHeavyAttack();
                break;
            case TouchType.HoldRelase:
                ReleaseHeavyAttack();
                break;
            case TouchType.Dash:
                TriggerDash();
                break;
            default:
                CheckJoysticInput();
                break;
        }

        
    }

	void FixedUpdate () {
        switch (controlState)
        {
            case ControlState.Idle:
                break;
            case ControlState.Moving: {
                float sqrDistToAttackMark = Vector3.SqrMagnitude(transform.position - attackMark);
                float sqrdistanceToStopOnTap = distanceToStopOnTap * distanceToStopOnTap;

                if(sqrDistToAttackMark <= sqrdistanceToStopOnTap) {
                    controlState = ControlState.Idle;
                } else {
                    playerMotion.LookAt(attackMark);
                    playerMotion.Advance();
                }
                break;
            }
            case ControlState.Attacking: {
                float sqrDistToAttackMark = Vector3.SqrMagnitude(transform.position - attackMark);
                float sqrDistanceToAttackOnTap = distanceToAttackOnTap * distanceToAttackOnTap;
                if(sqrDistToAttackMark <= distanceToAttackOnTap) {
                    TriggerAttack(attackMark);
                    controlState = ControlState.Idle;
                } else {
                    playerMotion.LookAt(attackMark);
                    playerMotion.Advance();
                }
                break;
            }
            default:
                break;
        }
    }

    TouchType CheckTouchType()
    {
        float elapsedTime = Time.realtimeSinceStartup - pressBegin;
        bool isHolding = pressBegin != -1 && elapsedTime >= timeToEnterInHolding;
        TouchType result = TouchType.None;

        if (Input.GetMouseButtonDown(0))
        {
            pressBegin = Time.realtimeSinceStartup;
            pressPosition = Input.mousePosition;
        }

        if (isHolding)
        {
            result = TouchType.Holding;
        }

        if (Input.GetMouseButtonUp(0))
        {
            pressBegin = -1;

            Vector3 raisePosition = Input.mousePosition;
            Vector3 dashScreenVector = raisePosition - pressPosition;
            float swipeDistance = Vector3.Magnitude(dashScreenVector) / Screen.dpi;

            if (isHolding)
            {
                result = TouchType.HoldRelase;
            }
            else if(swipeDistance >= distanceToConsiderDash)
            {
                Vector3 raisePositionWorld = GetScreenPositonProjectedOnFloor(raisePosition);
                Vector3 pressPositionWorld = GetScreenPositonProjectedOnFloor(pressPosition);
                lastDashWorldVector = raisePositionWorld - pressPositionWorld;
                lastDashWorldVector.y = 0;
                result = TouchType.Dash;
            }
            else
            {
                result = TouchType.Tap;
            }
        }

        return result;
    }

    void SetAttackOrMoveMode()
    {
        attackMark = GetScreenPositonProjectedOnFloor(Input.mousePosition);
        PlayerMain enemy = GetEnemyOnScreenPosition(Input.mousePosition);
        if(enemy == null) {
            controlState = ControlState.Moving;
        } else {
            controlState = ControlState.Attacking;
        }
    }

    void TriggerAttack(Vector3 position) {
        playerAttack.Attack(position);
            controlState = ControlState.Idle;
    }

    void TriggerDash() {
        controlState = ControlState.Idle;
        playerAttack.Dash(lastDashWorldVector.normalized);
    }

    void ChargeHeavyAttack() {
        controlState = ControlState.Idle;
        playerAttack.ChargeHeavyAttack();
    }

    void ReleaseHeavyAttack() {
        controlState = ControlState.Idle;
        playerAttack.ReleaseHeavyAttack();
    }
    
    Vector3 GetScreenPositonProjectedOnFloor(Vector3 screenPosition) {
        Ray screenRay = GetRayFromScreenPosition(screenPosition);
        Plane groundPlane = new Plane(transform.up, transform.position);

        float hitDistance;
        if (groundPlane.Raycast(screenRay, out hitDistance))
        {
            return screenRay.GetPoint(hitDistance);
        }
        else throw new Exception(String.Format(
            "Failed projection of the screen point {0}", screenPosition
        ));
    }

    PlayerMain GetEnemyOnScreenPosition(Vector3 screenPosition) {
        Ray screenRay = GetRayFromScreenPosition(screenPosition);

        RaycastHit hit;
        if (!Physics.SphereCast(screenRay, tapRayRadius, out hit, Mathf.Infinity, playersLayerMask)) {
            return null;
        }

        PlayerMain player = hit.collider.GetComponent<PlayerMain>();

        if(player == null) {
            return null;
        }

        return player;
    }

    Ray GetRayFromScreenPosition(Vector3 screenPosition) {
        Ray screenRay;

        screenPosition.z = -mainCamera.nearClipPlane;
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        if(mainCamera.orthographic) {
            screenRay = new Ray(worldPosition, mainCamera.transform.forward);
        } else {
            Vector3 direction = mainCamera.transform.position - worldPosition;
            screenRay = new Ray(mainCamera.transform.position, direction);
        }

        return screenRay;
    }

    void CheckJoysticInput()
    {
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(xAxis, 0, yAxis);

        if (direction.sqrMagnitude >= axisVectorDeadZone * axisVectorDeadZone)
        {
            playerMotion.LookTowards(direction);
            playerMotion.Advance();
            controlState = ControlState.Idle;
        }
        else
        {
            playerMotion.Stop();
        }
    }
}
