using System;
using System.Collections;
using DarkWinter.Util.DataStructures;
using UnityEngine;

namespace HackNSlash.Player.AIActions
{
    public class DetectEnemies : LAction
    {
        private ArrayList enemiesInSight;
        //private BattleGameScene battleGameScene;
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
            //this.battleGameScene = battleGameScene;
            this.sightColliderPub = sightColliderPub;
            this.ai = ai;
            this.playerMain = ai.GetComponent<PlayerMain>();

            return this;
        }

        protected override void _OnStart()
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

            if(enemiesInSight.Count > 0)
            {
                LockTarget((PlayerMain) enemiesInSight[0]);
            }
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
                AddEnemy(player);
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
                RemoveEnemy(player);
            }
        }

        protected void AddEnemy(PlayerMain enemy)
        {
            PlayerConstitution constitution = enemy.GetComponent<PlayerConstitution>();
            if (constitution == null) {
                throw new Exception(String.Format(
                    "{0} expected to have PlayerConstitution", enemy
                ));
            }
            constitution.OnDieEvent += OnEmenyDie;

            enemiesInSight.Add(enemy);
        }

        protected void RemoveEnemy(PlayerMain enemy)
        {
            PlayerConstitution constitution = enemy.GetComponent<PlayerConstitution>();
            if (constitution == null)
            {
                throw new Exception(String.Format(
                    "{0} expected to have PlayerConstitution", enemy
                ));
            }
            constitution.OnDieEvent -= OnEmenyDie;

            enemiesInSight.Remove(enemy);
        }

        protected void OnEmenyDie(PlayerMain playerMain, float lastHit)
        {
            RemoveEnemy(playerMain);
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
