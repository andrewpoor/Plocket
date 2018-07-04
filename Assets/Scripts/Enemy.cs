using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamageable {

   public int health;

   protected bool alive = true;
   protected Animator animator;
   protected AudioSource audioSource;

   private bool FinishedExplodeAnimation = false;
   private bool FinishedExplodeAudio = false;

   protected virtual void Start () {
      animator = GetComponent<Animator> ();
      audioSource = GetComponent<AudioSource> ();
      GameManager.Instance.RegisterEnemy (this);
   }

   public void DealDamage(int damage) {
      health -= damage;

      if (health <= 0) {
         GetComponent<Collider2D> ().enabled = false;
         alive = false;
         audioSource.Play ();
         Invoke ("OnExplodeAudioFinished", audioSource.clip.length);
         animator.SetTrigger ("Explode"); //Event trigger at end calls OnDestruction
         StartCoroutine(WaitUntilDestroyed());
      } else {
         animator.SetTrigger ("Damage");
         ReactToDamage();
      }
   }

   //A hook for child classes to make use of if needed.
   protected virtual void ReactToDamage() {
      //Do nothing by default.
   }

   private IEnumerator WaitUntilDestroyed() {
      while (!FinishedExplodeAnimation || !FinishedExplodeAudio) {
         yield return null;
      }

      gameObject.SetActive (false);
   }

   private void OnDestruction() {
      GetComponent<Renderer> ().enabled = false;
      GetComponent<Animator>().enabled = false;
      FinishedExplodeAnimation = true;

      if(GameManager.Instance.player.alive) {
         GameManager.Instance.EnemyDestroyed(this);
      }
   }

   private void OnExplodeAudioFinished() {
      FinishedExplodeAudio = true;
   }
}
