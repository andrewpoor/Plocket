using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingEnemy : EnemyType {

   [SerializeField] private float speed;

   void FixedUpdate () {
      if(Time.timeScale > float.Epsilon) {
         //Move directly towards the player.
         Vector2 direction = GameManager.Instance.Player.transform.position - transform.position;
         transform.Translate(direction.normalized * speed * Time.deltaTime);
      }
   }

   void OnTriggerEnter2D(Collider2D other)
   {
      //Explode on contact with player
      if (other.CompareTag("Player"))
      {
         enemyBehaviour.Explode();
      }
   }
}
