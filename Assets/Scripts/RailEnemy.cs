using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailEnemy : Enemy {

   public float speed;
   public Vector2 direction;

   void FixedUpdate () {
      transform.Translate (direction.normalized * speed * Time.deltaTime);
   }

   void OnTriggerEnter2D(Collider2D other) {
      if (other.CompareTag ("Obstacle") || other.CompareTag("Damageable")) {
         direction *= -1;
      }
   }
}
