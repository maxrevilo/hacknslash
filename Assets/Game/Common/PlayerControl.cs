using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMain))]
public class PlayerControl : MonoBehaviour {
	private PlayerMain playerMain;

	private Camera mainCamera;

    [SerializeField]
    private TouchType tt;

    [SerializeField]
    private float axisVectorDeadZone = 0.1f;
    [SerializeField]
    private float timeToEnterInHolding = 0.3f;

    float pressBegin = -1;

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
        tt = CheckTouchType();
        switch (tt)
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
        }

        if (isHolding)
        {
            result = TouchType.Holding;
        }

        if (Input.GetMouseButtonUp(0))
        {
            pressBegin = -1;
            if (isHolding)
            {
                result = TouchType.HoldRelase;
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
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 1;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        Ray mouseRay = new Ray(worldPosition, mainCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit))
        {
            playerMain.Attack(hit.point);
        }
        else
        {
            Debug.LogError("Failed projection of the mouse");
        }
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
