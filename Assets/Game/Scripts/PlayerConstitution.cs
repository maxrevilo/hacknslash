using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMain))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerConstitution : MonoBehaviour {
    
    public delegate void LifeChangeEvent(PlayerMain playerMain, float current, float previous);
    public event LifeChangeEvent OnLifeChangeEventEvent;
    
    public delegate void AttackedEvent(PlayerMain playerMain, PlayerWeaponDef weapon);
    public event AttackedEvent OnAttackedEvent;
    
    public delegate void DieEvent(PlayerMain playerMain, float lastHit);
    public event DieEvent OnDieEvent;
    
    public bool spawnLifeBar = true;
    
    private PlayerMain playerMain;

    public float defHitPoints = 100;
    
    public string DeadBodyPrefabName;
    
    private GameObject lifeBarGO;
    
    private Rigidbody _rigidBody;

    void Awake()
    {
        playerMain = GetComponent<PlayerMain>();
        _rigidBody = GetComponent<Rigidbody>();
    }

    void Start() {
        hitPoints = defHitPoints;
        StartCoroutine(InitiateLifeBar());
    }

    private IEnumerator InitiateLifeBar() {
        yield return new WaitForFixedUpdate();
        
        if(spawnLifeBar) {
            lifeBarGO = PoolingSystem.Instance.InstantiateAPS(
                "LifeBar",
                Vector3.zero,
                Quaternion.identity,
                transform.parent.gameObject
            );
            
            LifeBar lifeBar = lifeBarGO.GetComponent<LifeBar>();
            lifeBar.playerConstitution = this;
        }

        AddHitPoints(0);
    }
    
    public float hitPoints { get; private set; }

    public void DamageWith(PlayerWeaponDef weapon)
    {
        if(OnAttackedEvent != null) OnAttackedEvent(playerMain, weapon);
        this.AddHitPoints(-weapon.attackDmg);
    }

    public void AddHitPoints(float hitPointsDiff)
    {
        float newHitPoints = hitPoints + hitPointsDiff;
        
        if (newHitPoints > defHitPoints)
        {
            newHitPoints = defHitPoints;
        }
        else if(newHitPoints <= 0)
        {
            newHitPoints = 0;
        }
        
        if(OnLifeChangeEventEvent != null) {
            OnLifeChangeEventEvent(playerMain, newHitPoints, hitPoints);
        }
        
        hitPoints = newHitPoints;
        if (hitPoints == 0) {
            if(OnDieEvent != null) OnDieEvent(playerMain, hitPointsDiff);
            Die();
        }
    }

    public float LifeFraction()
    {
        return hitPoints / defHitPoints;
    }

    public void Die()
    {
        StartCoroutine(DieCorruoutine());
    }
    
    private IEnumerator DieCorruoutine() {
        yield return new WaitForFixedUpdate();
        
        gameObject.DestroyAPS();
        if(lifeBarGO != null) {
            lifeBarGO.DestroyAPS();
        }

        if(DeadBodyPrefabName != null) {
            GameObject deadBodyGO = PoolingSystem.Instance.InstantiateAPS(
                DeadBodyPrefabName,
                transform.position,
                transform.rotation,
                transform.parent.gameObject
            );
            
            Rigidbody deadBodyRB = deadBodyGO.GetComponentInChildren<Rigidbody>();
            deadBodyRB.velocity = this._rigidBody.velocity;
            deadBodyRB.angularVelocity = this._rigidBody.angularVelocity;
        }
    }
}