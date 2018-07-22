using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Unifies the various enemy types so that they can be used interchangibly by other scripts.
//Inheritance allows for enemy type scripts to be disabled by other scripts without need for casting.
public abstract class EnemyType : MonoBehaviour {
   protected Enemy enemyBehaviour; //Reference to own Enemy script.
   protected Animator animator;
   protected AudioSource audioSource;

   protected virtual void Start() {
      enemyBehaviour = GetComponent<Enemy>();
      animator = GetComponent<Animator>();
      audioSource = GetComponent<AudioSource>();
   }

   public virtual void ReactToDamage() {
      //Do nothing by default.
   }
}