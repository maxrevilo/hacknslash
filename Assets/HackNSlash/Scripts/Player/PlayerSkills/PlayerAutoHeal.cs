using UnityEngine;

[RequireComponent(typeof(PlayerConstitution))]
public class PlayerAutoHeal : Resetable
{
    [SerializeField]
    private float healPerSecond = 10;
    [SerializeField]
    private float timeBeforeHealing = 15f;

    private CountDown timeBeforeHealingCD;

    private PlayerConstitution playerConstitution;

    protected override void Awake()
    {
        playerConstitution = GetComponent<PlayerConstitution>();
        base.Awake();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if(timeBeforeHealingCD.HasFinished())
        {
            playerConstitution.AddHitPoints(healPerSecond * Time.deltaTime);
        }
    }

    private void OnAttacked(HitArea hitArea)
    {
        timeBeforeHealingCD.Restart(timeBeforeHealing);
    }

    protected override void _Reset()
    {
        playerConstitution.OnAttackedEvent -= OnAttacked;
        playerConstitution.OnAttackedEvent += OnAttacked;

        timeBeforeHealingCD.Stop();
    }
}
