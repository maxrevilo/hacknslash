using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class SoftCollision : MonoBehaviour {

    [SerializeField]
    private float exponent = 1.7f;
    [SerializeField]
    private float factor = 0.7f;

    private Rigidbody rigidBody;

    private ArrayList softCollisions;

    private void Awake()
    {
        softCollisions = new ArrayList(5);
        rigidBody = this.transform.parent.GetComponent<Rigidbody>();
    }


    void FixedUpdate()
    {
        foreach(SoftCollision softCollision in softCollisions)
        {
            Debug.DrawLine(transform.position + new Vector3(0, 2, 0), softCollision.transform.position + new Vector3(0, 2, 0), Color.cyan);

            Transform otherTransform = softCollision.transform;

            Vector3 fromToVec = otherTransform.position - transform.position;

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
}