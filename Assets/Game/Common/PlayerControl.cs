using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMain))]
public class PlayerControl : MonoBehaviour {
	private PlayerMain playerMain;

	private Camera mainCamera;

    public float axisVectorDeadZone = 0.1f;
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
        switch (CheckTouchType())
        {
            case TouchType.Tap:
                TriggerAtack();
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
        float timeToEnterInHolding = 0.3f;
        TouchType result = TouchType.None;

        if (Input.GetMouseButtonDown(0))
        {
            pressBegin = Time.realtimeSinceStartup;
        }

        if (elapsedTime >= timeToEnterInHolding)
        {
            result = TouchType.Holding;
        }

        if (Input.GetMouseButtonUp(0))
        {
            pressBegin = -1;
            if (elapsedTime >= timeToEnterInHolding)
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
