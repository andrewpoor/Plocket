using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyShot : MonoBehaviour {

   public float speed;
   public int damage;

   private Animator animator;

   void Start() {
      animator = GetComponent<Animator> ();
   }

   void FixedUpdate () {
      transform.Translate (Vector2.up * speed * Time.deltaTime);
   }

   void OnTriggerEnter2D(Collider2D other) {
      if (other.CompareTag ("Damageable")) {
         IDamageable hitObject = other.GetComponent<IDamageable> ();
         hitObject.DealDamage (damage);
         animator.SetTrigger ("Explode"); //Event trigger at end calls EndExplode
         speed = 0;
      } else if (other.CompareTag ("Obstacle")) {
         animator.SetTrigger ("Explode"); //Event trigger at end calls EndExplode
         speed = 0;
      }
   }

   private void EndExplode() {
      Destroy (gameObject);
   }
}
