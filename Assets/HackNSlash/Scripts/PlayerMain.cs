using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMain : Resetable
{
    private Rigidbody playerRigidBody;

    protected override void Awake()
    {
        base.Awake();
        playerRigidBody = GetComponent<Rigidbody>();
    }

    protected override void Start() {
        base.Start();
    }

    protected override void _Reset()
    {
    }

    // Update is called once per frame
    protected override void Update() {
        base.Update();
    }

    #region Teams
    public int team = 1;

    public bool isAlly(PlayerMain otherPlayer) {
        return otherPlayer.team == this.team;
    }
    #endregion
}
