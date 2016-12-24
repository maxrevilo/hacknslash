using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MovementEffects;
using System;

[RequireComponent(typeof(Collider))]
public class SoftCollision : Resetable {

    [SerializeField]
    private float exponent = 1.7f;
    [SerializeField]
    private float factor = 0.7f;

    private Rigidbody rigidBody;

    private ArrayList softCollisions;

    protected override void Awake()
    {
        base.Awake();
        softCollisions = new ArrayList(5);
        rigidBody = this.transform.parent.GetComponent<Rigidbody>();
    }

    protected override void _Reset()
    {
        softCollisions.Clear();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        foreach(SoftCollision softCollision in softCollisions)
        {
            if(!softCollision.isActiveAndEnabled)
            {
                //Debug.LogFormat("Removing soft collision {0} by death.", softCollision.transform.parent.gameObject.name);
                Timing.RunCoroutine(_RemoveCollisionAsync(softCollision), Segment.FixedUpdate);
                continue;
            }

            Debug.DrawLine(transform.position + new Vector3(0, 2, 0), softCollision.transform.position + new Vector3(0, 2, 0), Color.cyan);

            Transform otherTransform = softCollision.transform;

            Vector3 fromToVec = otherTransform.position - transform.position;

            // We want to avoid pushing objects towards the air, so we will remove the vertical component.
            fromToVec.y *= 0f;

            float invDistance = 1f / fromToVec.magnitude;

            Vector3 direction = fromToVec.normalized;

            float repulsion = Mathf.Pow(invDistance, exponent) * factor;

            rigidBody.AddForce(-direction * repulsion, ForceMode.VelocityChange);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SoftCollision otherSoftCollision = other.GetComponent<SoftCollision>();

        if (otherSoftCollision != null)
        {
            AddNewCollision(otherSoftCollision);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SoftCollision otherSoftCollision = other.GetComponent<SoftCollision>();
        if(otherSoftCollision != null)
        {
            RemoveCollision(otherSoftCollision);
        }
    }

    private bool HasCollision(SoftCollision softCollisionToFind)
    {
        return softCollisions.Contains(softCollisionToFind);
    }

    private bool AddNewCollision(SoftCollision newSoftCollision)
    {
        if(!HasCollision(newSoftCollision))
        {
            softCollisions.Add(newSoftCollision);
            return true;
        }
        return false;
    }

    private void RemoveCollision(SoftCollision newSoftCollision)
    {
        softCollisions.Remove(newSoftCollision);
    }

    private IEnumerator<float> _RemoveCollisionAsync(SoftCollision softCollision)
    {
        yield return 0f;
        RemoveCollision(softCollision);
    }
}