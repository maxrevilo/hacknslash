using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMain))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(PlayerStability))]
public class PlayerMotion : Resetable
{
    public delegate void MovingEvent(PlayerMain playerMain, bool moving);
    public event MovingEvent OnMovingEvent;

    [SerializeField]
    private float defSpeed = 2;
    private bool advancing;
    [SerializeField]
    private float defRotationSpeed = 15;
    private Quaternion targetDirection;

    private Rigidbody playerRigidBody;
    private PlayerMain playerMain;
    private PlayerAttack playerAttack;
    private PlayerStability playerStability;


    protected override void Awake()
    {
        base.Awake();
        playerMain = GetComponent<PlayerMain>();
        playerRigidBody = GetComponent<Rigidbody>();
        playerAttack = GetComponent<PlayerAttack>();
        playerStability = GetComponent<PlayerStability>();
    }

    protected override void Start(){
        base.Start();
    }

    protected override void _Reset()
    {
        advancing = false;
        targetDirection = Quaternion.identity;
    }

    protected override void Update(){
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (playerStability.IsStable())
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetDirection, Time.deltaTime * defRotationSpeed
            );
            if (advancing)
            {
                playerRigidBody.MovePosition(
                    transform.position + defSpeed * Time.fixedDeltaTime * transform.forward
                );
            }
        }
    }

    public void Advance() {
        if (!playerAttack.IsAtacking())
        {
            advancing = true;
            if (OnMovingEvent != null) OnMovingEvent(playerMain, true);
        }
    }

    public void Stop() {
        advancing = false;
        if (OnMovingEvent != null) OnMovingEvent(playerMain, false);
    }

    public void LookAt(Vector3 position, bool force = false) {
        if (force || !playerAttack.IsAtacking())
        {
            LookTowards(position - transform.position, force);
        }
    }

    public void LookTowards(Vector3 direction, bool force = false, bool immediate = false)
    {
        if (force || !playerAttack.IsAtacking())
        {
            direction.y = 0;
            targetDirection = Quaternion.LookRotation(direction, transform.up);
            if(immediate)
            {
                transform.rotation = targetDirection;
            }
        }
    }
}
