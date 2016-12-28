using System.Collections;
using DarkWinter.Util.DataStructures;
using UnityEngine;

namespace HackNSlash.Player.AIActions
{
    public class DetectEnemies : LAction
    {
        private ArrayList enemiesInSight;
        private BattleGameScene battleGameScene;
        private CollisionPub sightColliderPub;
        private PlayerMain playerMain;
        private EnemyAI ai;

        public DetectEnemies()
        {
            isBlocking = true;
            lanes = (uint) BipedPlayerLanes.Main;
            enemiesInSight = new ArrayList();
        }

        public DetectEnemies Initialize(EnemyAI ai, BattleGameScene battleGameScene,
            CollisionPub sightColliderPub)
        {
            this.battleGameScene = battleGameScene;
            this.sightColliderPub = sightColliderPub;
            this.ai = ai;
            this.playerMain = ai.GetComponent<PlayerMain>();

            return this;
        }

        public override void _OnStart()
        {
            enemiesInSight.Clear();

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
        }

        bool isAlly(PlayerMain player) { return player.team == playerMain.team; }

        void OnSight(Collider other)
        {
            PlayerMain player = other.GetComponent<PlayerMain>();

            if (player == null) return;

            if (isAlly(player))
            {

            }
            else
            {
                enemiesInSight.Add(player);

                if(!isBlocked) {
                    // TODO: T1. This should be a procedure to look on the
                    // enemiesInSight list for the best target to lock on
                    LockTarget(player);
                }
            }
        }

        void OutOfSight(Collider other)
        {
            PlayerMain player = other.GetComponent<PlayerMain>();
            if (player == null) return;

            if (isAlly(player))
            {

            }
            else
            {
                enemiesInSight.Remove(player);
            }
        }

        void LockTarget(PlayerMain player)
        {   
            InsertInFrontOfMe(
                EngageEnemy.Create().Initialize(ai, player, sightColliderPub)
            );
        }

        protected static ObjectsPool<DetectEnemies> pool;
        public static DetectEnemies Create() {
            if(pool == null) {
                pool = new ObjectsPool<DetectEnemies>(()=>new DetectEnemies(), 1);
            }
            DetectEnemies instance = pool.retreive();
            return instance;
        }
    }
}
