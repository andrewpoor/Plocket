using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingEnemy : Enemy {

   public float speed;

   void FixedUpdate () {
      if (alive) {
         //Move directly towards the player.
         Vector2 direction = GameManager.Instance.player.transform.position - transform.position;
         transform.Translate (direction.normalized * speed * Time.deltaTime);
      }
   }

   void OnTriggerEnter2D(Collider2D other)
   {
      //Explode on contact with player
      if (other.CompareTag("Player"))
      {
         DealDamage(health);
      }
   }
}
