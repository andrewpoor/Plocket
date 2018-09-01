using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Unifies the various enemy types so that they can be used interchangibly by other scripts.
//Inheritance allows for enemy type scripts to be disabled by other scripts without need for casting.
public abstract class EnemyType : MonoBehaviour {
   [SerializeField] protected Enemy enemyBehaviour; //Reference to own Enemy script.
   [SerializeField] protected Animator animator;
   [SerializeField] protected AudioSource audioSource;

   protected virtual void Start() {
      enemyBehaviour = GetComponent<Enemy>();
      animator = GetComponent<Animator>();
      audioSource = GetComponent<AudioSource>();
   }

   //Hook for enemy types to respond whenever damage is taken.
   public virtual void ReactToDamage() {
      //Do nothing by default.
   }

   //Hook for enemy types to do something after exploding, before the game object is destroyed.
   public virtual void ReactToExplode() {
      //Do nothing by default.
   }

   private void OnDisable() {
      StopAllCoroutines();
   }
}