using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour {

   private const float SPIN_DEGREES = 360.0f;

   public AudioClip launchAudio;
   public AudioClip fireAudio;
   public AudioClip explodeAudio;

   public float launchingSpeed; //Speed when initially coming out of the boss.
   public float firingSpeed; //Speed when firing towards the player.
   public float numberSpins; //Number of spins the rocket does before firing towards the player.
   public float spinSpeed; //At a rate of spins per second.
   public Vector2 initialDisplacement; //Amount the rocket flies out of the boss before changing directions for the player.

   private Animator animator;
   private PolygonCollider2D polygonCollider;
   private AudioSource audioSource;

   void Start () {
      animator = GetComponent<Animator>();
      polygonCollider = GetComponent<PolygonCollider2D>();
      audioSource = GetComponent<AudioSource>();
      StartCoroutine(Launch());
	}

   //The rocket first projects out of the boss while spinning for a while.
   //It then angles towards the player and shoots straight at them.
   private IEnumerator Launch() {
      //Leave the launcher
      PlayClip(launchAudio);
      Vector3 target = transform.position + (Vector3) initialDisplacement;
      while ((transform.position - target).sqrMagnitude > float.Epsilon) {
         Vector2 newPosition = Vector2.MoveTowards(transform.position, target, launchingSpeed * Time.deltaTime);
         transform.position = newPosition;
         transform.Rotate(new Vector3(0, 0, SPIN_DEGREES * spinSpeed * Time.deltaTime));
         yield return null;
      }

      //Spin in place a few times
      float timer = 0;
      float totalTime = numberSpins / spinSpeed;
      while(timer < totalTime) {
         timer += Time.deltaTime;
         transform.Rotate(new Vector3(0, 0, SPIN_DEGREES * spinSpeed * Time.deltaTime));
         yield return null;
      }

      //Spin to face the player
      Vector3 rocketToPlayer = GameManager.Instance.player.transform.position - transform.position;
      float startingAngle = transform.rotation.eulerAngles.z;
      float targetAngle = Mathf.Atan2(rocketToPlayer.y, rocketToPlayer.x) * Mathf.Rad2Deg - 90.0f;
      targetAngle = (targetAngle + SPIN_DEGREES) % SPIN_DEGREES;
      timer = 0;
      totalTime = (normaliseAngle(targetAngle - startingAngle)  / SPIN_DEGREES) / spinSpeed;
      while (timer < totalTime) {
         timer += Time.deltaTime;
         transform.Rotate(new Vector3(0, 0, SPIN_DEGREES * spinSpeed * Time.deltaTime));
         yield return null;
      }

      //Fire at the player
      animator.SetTrigger("Fire");
      PlayClip(fireAudio);
      while(true) {
         transform.Translate(Vector2.up * firingSpeed * Time.deltaTime);
         yield return null;
      }
   }

   private float normaliseAngle(float angle) {
      return (angle + SPIN_DEGREES) % SPIN_DEGREES;
   }

   void OnTriggerEnter2D(Collider2D other) {
      if (other.CompareTag("Player") || other.CompareTag("Obstacle")) {
         StartExplode();
      }
   }

   //The rocket has hit something, and so begins to explode.
   private void StartExplode() {
      animator.SetTrigger("Explode"); //Event trigger at end calls EndExplode
      PlayClip(explodeAudio);
      firingSpeed = 0;
      polygonCollider.enabled = false;
   }

   private void EndExplode() {
      Destroy(gameObject);
   }

   private void PlayClip(AudioClip clip) {
      audioSource.clip = clip;
      audioSource.Play();
   }
}
