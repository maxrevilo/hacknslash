using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMain : MonoBehaviour {
    private Rigidbody playerRigidBody;
    
    void Awake()
    {
        playerRigidBody = GetComponent<Rigidbody>();
        #region Health
        lifeBar = GetComponentInChildren<LifeBar>();
        #endregion
    }

    void Start() {
        #region Health
        hitPoints = defHitPoints;
        #endregion
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

    #region Health
    private LifeBar lifeBar;

    [SerializeField]
    private float defHitPoints = 100;

    public float hitPoints { get; private set; }

    public void DamageWith(PlayerWeaponDef weapon)
    {
        this.AddHitPoints(-weapon.attackDmg);
    }

    public void AddHitPoints(float hitPointsDiff)
    {
        this.hitPoints += hitPointsDiff;
        if(lifeBar)
        {
            lifeBar.setValue(LifeFraction());
        }

        if (hitPoints > defHitPoints)
        {
            hitPoints = defHitPoints;
        } else if(hitPoints <= 0)
        {
            Die();
        }
    }

    public float LifeFraction()
    {
        return hitPoints / defHitPoints;
    }

    public void Die()
    {
        gameObject.DestroyAPS();
    }
    #endregion

    #region Physics
    
    public void Push(Vector3 pushVector)
    {
        playerRigidBody.velocity += pushVector;
    }
    #endregion
}
