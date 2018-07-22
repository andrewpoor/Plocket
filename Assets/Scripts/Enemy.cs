using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Generic enemy behaviour.
//Kept separate from more specific behaviour so that it can be disabled separately.
public class Enemy : MonoBehaviour, IDamageable {

   public AudioClip explodeAudio;
   public int health;

   [HideInInspector] public bool alive = false;
   [HideInInspector] public bool spawnIn = false;

   private Animator animator;
   private AudioSource audioSource;
   private EnemyType enemyTypeBehaviour; //Reference to own specific enemy type script.
   private bool FinishedExplodeAnimation = false;
   private bool FinishedExplodeAudio = false;

   private void Start () {
      animator = GetComponent<Animator> ();
      audioSource = GetComponent<AudioSource> ();
      enemyTypeBehaviour = GetComponent<EnemyType>();
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
      enemyTypeBehaviour.ReactToDamage();

      if (health <= 0) {
         Explode();
      } else {
         animator.SetTrigger ("Damage");
      }
   }

   //Starts the enemie's destruction sequence.
   //Disables certain behaviours, plays appropriate visuals and sounds.
   public void Explode() {
      alive = false;
      GetComponent<Collider2D>().enabled = false;
      enemyTypeBehaviour.enabled = false;
      audioSource.clip = explodeAudio;
      audioSource.Play();
      Invoke("OnExplodeAudioFinished", audioSource.clip.length);
      animator.SetTrigger("Explode"); //Event trigger at end calls OnDestruction
      StartCoroutine(WaitUntilDestroyed());
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
