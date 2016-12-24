using System;
using UnityEngine;

[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(PlayerMotion))]
public class PlayerControl : Resetable {
	
    //private PlayerMain playerMain;
    private PlayerAttack playerAttack;
    private PlayerMotion playerMotion;

	private Camera mainCamera;

    [SerializeField]
    private float axisVectorDeadZone = 0.1f;
    [SerializeField]
    private float timeToEnterInHolding = 0.3f;
    [SerializeField]
    private float distanceToConsiderDash = 0.3f;

    private float pressBegin;
    private Vector3 pressPosition;
    private Vector3 lastDashWorldVector;

    private enum TouchType
    { None, Tap, Holding, HoldRelase, Dash, HeldDash }

    [SerializeField]
    private bool useJoystick = false;

    protected override void Awake() {
        base.Awake();
		playerAttack = GetComponent<PlayerAttack>();
		playerMotion = GetComponent<PlayerMotion>();
    }

    protected override void Start () {
        base.Start();
    }

    protected override void _Reset()
    {
        mainCamera = Camera.main;
        pressBegin = -1;
    }

    protected override void Update()
    {
        base.Update();
        if (useJoystick)
        {
            CheckJoysticInput();
            return;
        }

        switch (CheckTouchType())
        {
            case TouchType.Tap:
                SetAttackMode();
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
                break;
        }

        
    }

    protected override void FixedUpdate () {
        base.FixedUpdate();
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

    void SetAttackMode()
    {
        Vector3 attackMark = GetScreenPositonProjectedOnFloor(Input.mousePosition);
        TriggerAttack(attackMark);
        playerMotion.Stop();
    }

    void TriggerAttack(Vector3 position) {
        playerAttack.Attack(position);
    }

    void TriggerDash() {
        playerAttack.Dash(lastDashWorldVector.normalized);
    }

    void ChargeHeavyAttack() {
        playerAttack.ChargeHeavyAttack();
    }

    void ReleaseHeavyAttack() {
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
        }
        else
        {
            playerMotion.Stop();
        }
    }
}
