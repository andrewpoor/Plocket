using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamageable {

   public int health;

   protected bool alive = true;

   private Animator animator;
   private AudioSource audioSource;
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
         GetComponent<PolygonCollider2D> ().enabled = false;
         alive = false;
         audioSource.Play ();
         Invoke ("OnExplodeAudioFinished", audioSource.clip.length);
         animator.SetTrigger ("Explode"); //Event trigger at end calls OnDestruction
         StartCoroutine(WaitUntilDestroyed());
      } else {
         animator.SetTrigger ("Damage");
      }
   }

   private IEnumerator WaitUntilDestroyed() {
      while (!FinishedExplodeAnimation || !FinishedExplodeAudio) {
         yield return null;
      }

      gameObject.SetActive (false);
   }

   private void OnDestruction() {
      GameManager.Instance.EnemyDestroyed (this);
      GetComponent<Renderer> ().enabled = false;
      FinishedExplodeAnimation = true;
   }

   private void OnExplodeAudioFinished() {
      FinishedExplodeAudio = true;
   }
}
