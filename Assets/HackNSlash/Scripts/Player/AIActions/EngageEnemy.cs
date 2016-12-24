using System;
using System.Collections;
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

        public EngageEnemy(EnemyAI ai, PlayerMain target, CollisionPub sightColliderPub)
        {
            this.sightColliderPub = sightColliderPub;
            this.ai = ai;
            playerMotion = ai.GetComponent<PlayerMotion>();
            this.target = target;
        }

        public override void OnStart()
        {
            sightColliderPub.OnTriggerEnterEvent += OnSight;
            sightColliderPub.OnTriggerExitEvent += OutOfSight;
        }

        public override void OnEnd()
        {
            sightColliderPub.OnTriggerEnterEvent -= OnSight;
            sightColliderPub.OnTriggerExitEvent -= OutOfSight;
        }

        public override void Update(float deltaTime)
        {
            playerMotion.LookAt(target.transform.position);
            float distance = Vector3.Distance(target.transform.position, playerMotion.transform.position);
            if (distance >= ai.defAttackDistance)
            {
                playerMotion.Advance();
            }
            else
            {
                playerMotion.Stop();
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
    }
}
