using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailEnemy : EnemyType {

   [SerializeField] private float speed;
   [SerializeField] private Vector2 direction;

   void FixedUpdate () {
      if (Time.timeScale > float.Epsilon) {
         transform.Translate(direction.normalized * speed * Time.deltaTime);
      }
   }

   void OnTriggerEnter2D(Collider2D other) {
      //Reverse directions after colliding with something.
      if (other.CompareTag ("Obstacle") || other.CompareTag("Damageable")) {
         direction *= -1;
      }
   }
}
