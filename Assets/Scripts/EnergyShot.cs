using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyShot : MonoBehaviour {


   public int damage;

   [SerializeField] private float speed;
   [SerializeField] private Animator animator;
   [SerializeField] private CircleCollider2D circleCollider;

   void Start() {
      animator = GetComponent<Animator> ();
      circleCollider = GetComponent<CircleCollider2D> ();
   }

   void FixedUpdate () {
      transform.Translate (Vector2.up * speed * Time.deltaTime);
   }

   void OnTriggerEnter2D(Collider2D other) {
      if (other.CompareTag ("Damageable")) {
         IDamageable hitObject = other.GetComponent<IDamageable> ();
         hitObject.DealDamage (damage);
         StartExplode ();
      } else if (other.CompareTag ("Obstacle")) {
         StartExplode ();
      }
   }

   //The shot has hit something, and so begins to dissipate.
   private void StartExplode() {
      animator.SetTrigger ("Explode"); //Event trigger at end calls EndExplode
      speed = 0;
      circleCollider.enabled = false;
   }

   private void EndExplode() {
      Destroy (gameObject);
   }
}
