using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMain))]
public class PlayerControl : MonoBehaviour {
	private PlayerMain playerMain;

	private Camera mainCamera;

    [SerializeField]
    private float axisVectorDeadZone = 0.1f;
    [SerializeField]
    private float timeToEnterInHolding = 0.3f;
    [SerializeField]
    private float distanceToConsiderDash = 0.6f;

    private float pressBegin = -1;
    private Vector3 pressPosition;
    private Vector3 lastDashWorldVector;

    private enum TouchType
    { None, Tap, Holding, HoldRelase, Dash, HeldDash }
    

    void Awake() {
		playerMain = GetComponent<PlayerMain>();
		mainCamera = Camera.main;
	}

	void Start () {
        
    }

    void Update()
    {
        switch (CheckTouchType())
        {
            case TouchType.Tap:
                TriggerAtack();
                break;
            case TouchType.Holding:
                playerMain.ChargeHeavyAttack();
                break;
            case TouchType.HoldRelase:
                playerMain.ReleaseHeavyAttack();
                break;
            case TouchType.Dash:
                playerMain.Dash(lastDashWorldVector.normalized);
                break;
            default:
                CheckJoysticInput();
                break;
        }
    }

	void FixedUpdate () {
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

    void TriggerAtack()
    {
        Vector3 worldPosition = GetScreenPositonProjectedOnFloor(Input.mousePosition);
        playerMain.Attack(worldPosition);
    }
    
    Vector3 GetScreenPositonProjectedOnFloor(Vector3 screenPosition) {
        Ray screenRay;
        screenPosition.z = -mainCamera.nearClipPlane;
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        if(mainCamera.orthographic) {
            screenRay = new Ray(worldPosition, mainCamera.transform.forward);
        } else {
            Vector3 direction = mainCamera.transform.position - worldPosition;
            screenRay = new Ray(mainCamera.transform.position, direction);
        }

        RaycastHit hit;
        if (Physics.Raycast(screenRay, out hit))
        {
            return hit.point;
        }
        else throw new Exception(String.Format(
            "Failed projection of the screen point {0}", screenPosition
        ));
    }

    void CheckJoysticInput()
    {
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(xAxis, 0, yAxis);

        if (direction.sqrMagnitude >= axisVectorDeadZone * axisVectorDeadZone)
        {
            playerMain.LookTowards(direction);
            playerMain.Advance();
        }
        else
        {
            playerMain.Stop();
        }
    }
}
