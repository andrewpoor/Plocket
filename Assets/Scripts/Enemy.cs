using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamageable {

   public AudioClip explodeAudio;
   public int health;

   [HideInInspector] public bool alive = false;
   [HideInInspector] public bool spawnIn = false;

   protected Animator animator;
   protected AudioSource audioSource;

   private bool FinishedExplodeAnimation = false;
   private bool FinishedExplodeAudio = false;

   protected virtual void Start () {
      animator = GetComponent<Animator> ();
      audioSource = GetComponent<AudioSource> ();
      GameManager.Instance.RegisterEnemy (this);

      if(spawnIn) {
         alive = false;
         GetComponent<Collider2D>().enabled = false;
         animator.SetTrigger("Spawn"); //Event trigger at end calls EndSpawning
      } else {
         alive = true;
      }
   }

   private void EndSpawning() {
      alive = true;
      GetComponent<Collider2D>().enabled = true;
   }

   public void DealDamage(int damage) {
      health -= damage;

      if (health <= 0) {
         alive = false;
         GetComponent<Collider2D>().enabled = false;
         audioSource.clip = explodeAudio;
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
