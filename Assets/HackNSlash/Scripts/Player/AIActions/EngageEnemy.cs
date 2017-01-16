using DarkWinter.Util.DataStructures;
using UnityEngine;

namespace HackNSlash.Player.AIActions
{
    public class EngageEnemy : LAction
    {
        private CollisionPub sightColliderPub;
        private PlayerMotion playerMotion;
        private EnemyAI ai;
        private PlayerMain target;

        public EngageEnemy()
        {
            isBlocking = true;
            lanes = (uint) BipedPlayerLanes.Main;
        }

        public EngageEnemy Initialize(EnemyAI ai, PlayerMain target, CollisionPub sightColliderPub)
        {
            this.sightColliderPub = sightColliderPub;
            this.ai = ai;
            playerMotion = ai.GetComponent<PlayerMotion>();
            this.target = target;

            return this;
        }

        protected override void _OnStart()
        {
            sightColliderPub.OnTriggerEnterEvent += OnSight;
            sightColliderPub.OnTriggerExitEvent += OutOfSight;
        }

        protected override void _OnEnd()
        {
            sightColliderPub.OnTriggerEnterEvent -= OnSight;
            sightColliderPub.OnTriggerExitEvent -= OutOfSight;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            playerMotion.LookAt(target.transform.position);
            float distance = Vector3.Distance(target.transform.position, playerMotion.transform.position);
            if (distance >= ai.defAttackDistance * 0.7f)
            {
                playerMotion.Advance();
            }
            else
            {
                InsertInFrontOfMe(
                    AttackEnemy.Create().Initialize(ai, target)
                );
            }
        }

        void OnSight(Collider other)
        {}

        void OutOfSight(Collider other)
        {
            PlayerMain player = other.GetComponent<PlayerMain>();
            if (player == target)
            {
                // TODO: T2. The new version of LockTarget (as mentioned in DetectEnemies:T1)
                // should run here to look at for other enemies on sight.
                Disengage();
            }
        }

        void Disengage()
        {
            isFinished = true;
        }

        protected static ObjectsPool<EngageEnemy> pool;
        public static EngageEnemy Create() {
            if(pool == null) {
                pool = new ObjectsPool<EngageEnemy>(()=>new EngageEnemy(), 1);
            }
            EngageEnemy instance = pool.retreive();
            return instance;
        }
    }
}
