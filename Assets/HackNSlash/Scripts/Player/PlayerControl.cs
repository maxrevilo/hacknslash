using System;
using UnityEngine;
using Lean.Touch;

[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(PlayerMotion))]
public class PlayerControl : Resetable {
	
    //private PlayerMain playerMain;
    private PlayerAttack playerAttack;
    private PlayerMotion playerMotion;
    private PlayerSkillResource playerSkillResource;

    private Camera mainCamera;

    [SerializeField]
    private float timeToHeld = 0.3f;
    [SerializeField]
    private float minSwipeDistance = 0.3f;

    private bool capturingGesture;
    private CountDown heldCountDown;
    private Vector2 initialFingerPosition;
    private bool heldDetected;

    protected override void Awake() {
        base.Awake();
		playerAttack = GetComponent<PlayerAttack>();
		playerMotion = GetComponent<PlayerMotion>();
        heldCountDown = new CountDown();
        LeanTouch.OnFingerDown += OnFingerDown;
        LeanTouch.OnFingerSet += OnFingerSet;
        LeanTouch.OnFingerUp += OnFingerUp;
        playerSkillResource = GetComponent<PlayerSkillResource>();
    }

    protected override void OnDestroy() {
        LeanTouch.OnFingerDown -= OnFingerDown;
        LeanTouch.OnFingerSet -= OnFingerSet;
        LeanTouch.OnFingerUp -= OnFingerUp;
    }

    protected override void Start () {
        base.Start();
    }

    protected override void _Reset()
    {
        mainCamera = Camera.main;
        heldCountDown.Stop();
        capturingGesture = false;
        heldDetected = false;
    }

    void OnFingerDown(LeanFinger finger)
    {
        capturingGesture = true;
        initialFingerPosition = finger.ScreenPosition;
        heldCountDown.Restart(timeToHeld);
    }

    void OnFingerSet(LeanFinger finger)
    {
        if(capturingGesture)
        {
            bool captured = false;
            float swipeDistance = finger.GetScaledDistance(initialFingerPosition);
            if (swipeDistance >= minSwipeDistance)
            {

                DashDetected(initialFingerPosition, finger.ScreenPosition);
                captured = true;
            } else if(heldCountDown.HasFinished())
            {
                TapOnHoldDetected();
                heldDetected = true;
                captured = true;
            }

            if(captured)
            {
                heldCountDown.Stop();
                capturingGesture = false;
            }
        }
    }

    private void OnFingerUp(LeanFinger finger)
    {
        if(heldDetected)
        {
            HeldTapReleaseDetected();
        } else if(capturingGesture)
        {
            TapDetected();
        }
        capturingGesture = false;
        heldDetected = false;
        heldCountDown.Stop();
    }

    private void DashDetected(Vector2 initialScreenPosition, Vector2 finalScreenPosition)
    {
        TriggerDash(initialScreenPosition, finalScreenPosition);
    }

    private void TapDetected()
    {
        SetAttackMode();
    }

    private void TapOnHoldDetected()
    {
        if(playerSkillResource == null)
        {
            ChargeHeavyAttack();
        } else
        {
            if(playerSkillResource.amount > playerAttack.chargedWeaponDef.skillCost)
            {
                ForceReleaseHeavyAttack();
                playerSkillResource.AddToAmount(-playerAttack.chargedWeaponDef.skillCost);
            }
        }
    }

    private void HeldTapReleaseDetected()
    {
        if (playerSkillResource == null)
        {
            ReleaseHeavyAttack();
        }
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

    void TriggerDash(Vector2 initialScreenPosition, Vector2 finalScreenPosition) {
        Vector3 raisePositionWorld = GetScreenPositonProjectedOnFloor(finalScreenPosition);
        Vector3 pressPositionWorld = GetScreenPositonProjectedOnFloor(initialScreenPosition);
        Vector3 dashVector = raisePositionWorld - pressPositionWorld;
        dashVector.y = 0;
        playerAttack.Dash(dashVector.normalized);
    }

    void ForceReleaseHeavyAttack()
    {
        playerAttack.ForceReleaseHeavyAttack();
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
}
