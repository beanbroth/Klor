using System.Collections.Generic;
using UnityEngine;

public class S_BloodDecalSpawner : MonoBehaviour
{
    public GameObject bloodDecalPrefab; // Assign this in the Unity Inspector with your blood decal prefab
    private ParticleSystem particleSystem;

    // Use a List to store collision events
    private List<ParticleCollisionEvent> collisionEvents;
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = particleSystem.GetCollisionEvents(other, collisionEvents);
        for (int i = 0; i < numCollisionEvents; i++)
        {
            Vector3 pos = collisionEvents[i].intersection;
            Vector3 normal = collisionEvents[i].normal;

            GameObject decal = Instantiate(bloodDecalPrefab, pos, Quaternion.LookRotation(-normal));
            decal.transform.SetParent(other.transform); // Parent the decal to the collided object

            // Add random scale
            float randomScaleFactor = Random.Range(0.5f, 1.5f);
            decal.transform.localScale *= randomScaleFactor;

            // Add random rotation around the z-axis
            float randomRotation = Random.Range(0f, 360f);
            decal.transform.Rotate(Vector3.forward, randomRotation);

            
        }
    }


}