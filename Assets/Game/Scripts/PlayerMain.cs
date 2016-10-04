using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMain : MonoBehaviour {
    private Rigidbody playerRigidBody;
    
    void Awake()
    {
        playerRigidBody = GetComponent<Rigidbody>();
    }

    void Start() {
    }

    // Update is called once per frame
    void Update() {
    }

    #region Teams
    public int team = 1;

    public bool isAlly(PlayerMain otherPlayer) {
        return otherPlayer.team == this.team;
    }
    #endregion

    #region Physics
    
    public void Push(Vector3 pushVector)
    {
        playerRigidBody.AddForce(pushVector, ForceMode.VelocityChange);
    }
    #endregion
}
