using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;
using MovementEffects;

[RequireComponent(typeof(PlayerMain))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerConstitution : MonoBehaviour {
    
    public delegate void LifeChangeEvent(PlayerMain playerMain, float current, float previous);
    public event LifeChangeEvent OnLifeChangeEventEvent;
    
    public delegate void AttackedEvent(HitArea hitArea);
    public event AttackedEvent OnAttackedEvent;
    
    public delegate void DieEvent(PlayerMain playerMain, float lastHit);
    public event DieEvent OnDieEvent;

    public MonoBehaviour[] disableOnDead;
    
    public bool spawnLifeBar = true;
    
    private PlayerMain playerMain;

    public float defHitPoints = 100;
    
    public string DeadBodyPrefabName;

    public bool destroyOnDead = true;
    
    [SerializeField]
    public float hitPoints;

    [SerializeField]
    private bool ImpactTimeScaleDistortion = true;
    [SerializeField]
    private float ImpactCameraShake = 1.5f;

    private GameObject lifeBarGO;
    
    private Rigidbody _rigidBody;

    void Awake()
    {
        playerMain = GetComponent<PlayerMain>();
        _rigidBody = GetComponent<Rigidbody>();
    }

    void Start() {
        StartCoroutine(InitiateLifeBar());
        hitPoints = defHitPoints;
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

    public void DamageBy(HitArea hitArea)
    // TODO: Might be better to use a "Hit" object instead of the HitArea MonoBehaivor
    {
        PlayerWeaponDef weapon = hitArea.playerWeaponDef;

        if (OnAttackedEvent != null) OnAttackedEvent(hitArea);

        if(ImpactCameraShake > 0)
        {
            CameraShaker.Instance.ShakeOnce(ImpactCameraShake, 6f, 0.15f, 0.05f);
        }
        this.AddHitPoints(-weapon.attackDmg);

        Timing.RunCoroutine(_TimeSlowDown(), Segment.FixedUpdate);
    }

    private IEnumerator<float> _TimeSlowDown()
    {
        /*
        if(ImpactTimeScaleDistortion && Time.timeScale > 0.7f)
        {
            Time.timeScale = 0.1f;
            yield return Timing.WaitForSeconds(0.007f);
            Time.timeScale = 1f;
        }
        */
        return null;
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
        yield return new WaitForFixedUpdate();
        
        if(destroyOnDead) {
            gameObject.DestroyAPS();
        }

        if(lifeBarGO != null) {
            lifeBarGO.DestroyAPS();
        }

        if(DeadBodyPrefabName != null && DeadBodyPrefabName.Length > 0) {
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

        foreach(MonoBehaviour mb in disableOnDead) {
            mb.enabled = false;
        }
    }
}