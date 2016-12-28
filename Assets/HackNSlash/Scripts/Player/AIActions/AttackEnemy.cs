using DarkWinter.Util.DataStructures;
using UnityEngine;

namespace HackNSlash.Player.AIActions
{
    public class AttackEnemy : LAction
    {
        private PlayerAttack playerAttack;
        private PlayerMotion playerMotion;
        private EnemyAI ai;
        private PlayerMain target;
        private PlayerConstitution targetConstitution;
        private CountDown timeToAttack;

        public AttackEnemy()
        {
            isBlocking = true;
            lanes = (uint) BipedPlayerLanes.Main;
            timeToAttack = new CountDown();
        }

        public AttackEnemy Initialize(EnemyAI ai, PlayerMain target) {
            this.ai = ai;
            playerAttack = ai.GetComponent<PlayerAttack>();
            playerMotion = ai.GetComponent<PlayerMotion>();
            this.target = target;
            targetConstitution = target.GetComponent<PlayerConstitution>();
            timeToAttack.Stop();

            return this;
        }

        public override void _OnStart()
        {
            targetConstitution.OnDieEvent += TargetDied;
            timeToAttack.Restart(ai.defAttackDelay);
        }

        protected override void _OnEnd()
        {
            targetConstitution.OnDieEvent -= TargetDied;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            float distance = Vector3.Distance(target.transform.position, playerAttack.transform.position);
            if (distance >= ai.defAttackDistance)
            {
                Disengage();
            } else
            {
                playerMotion.Stop();
                if (timeToAttack.HasFinished())
                {
                    playerAttack.Attack(target.transform.position);
                    timeToAttack.Restart(ai.defAttackDelay);
                }
            }
        }

        void TargetDied(PlayerMain playerMain, float lastHit)
        {
            Disengage();
        }

        void Disengage()
        {
            isFinished = true;
        }

        protected static ObjectsPool<AttackEnemy> pool;
        public static AttackEnemy Create() {
            if(pool == null) {
                pool = new ObjectsPool<AttackEnemy>(()=>new AttackEnemy(), 1);
            }
            AttackEnemy instance = pool.retreive();
            return instance;
        }
    }
}
