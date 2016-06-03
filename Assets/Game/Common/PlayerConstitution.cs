using UnityEngine;

[RequireComponent(typeof(PlayerMain))]
public class PlayerConstitution : MonoBehaviour {
    // private PlayerMain playerMain;
    
    private LifeBar lifeBar;

    [SerializeField]
    private float defHitPoints = 100;

    void Awake()
    {
        // playerMain = GetComponent<PlayerMain>();
        lifeBar = GetComponentInChildren<LifeBar>();
    }

    void Start() {
        #region Health
        hitPoints = defHitPoints;
        #endregion
    }
    
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
}