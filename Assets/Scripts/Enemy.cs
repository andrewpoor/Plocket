using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamageable {

   public int health;

   private Animator animator;

   protected virtual void Start () {
      animator = GetComponent<Animator> ();
      GameManager.Instance.RegisterEnemy (this);
   }

   public void DealDamage(int damage) {
      health -= damage;

      if (health <= 0) {
         GetComponent<PolygonCollider2D> ().enabled = false;
         animator.SetTrigger ("Explode"); //Event trigger at end calls OnDestruction
      } else {
         animator.SetTrigger ("Damage");
      }
   }

   private void OnDestruction() {
      GameManager.Instance.EnemyDestroyed (this);
      gameObject.SetActive (false);
   }
}
