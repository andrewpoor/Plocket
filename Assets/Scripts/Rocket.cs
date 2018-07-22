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
      //Leave the launcher while spinning
      PlayClip(launchAudio);
      Vector3 target = transform.position + (Vector3) initialDisplacement;
      while ((transform.position - target).sqrMagnitude > float.Epsilon) {
         Vector2 newPosition = Vector2.MoveTowards(transform.position, target, launchingSpeed * Time.deltaTime);
         transform.position = newPosition;
         transform.Rotate(new Vector3(0, 0, SPIN_DEGREES * spinSpeed * Time.deltaTime));
         yield return null;
      }

      //Spin in place a few times
      float startingAngle = transform.rotation.eulerAngles.z;
      float targetAngle = startingAngle + numberSpins * SPIN_DEGREES;
      float totalTime = numberSpins / spinSpeed;
      yield return StartCoroutine(Turn(startingAngle, targetAngle, totalTime));

      //Spin to face the player
      startingAngle = transform.rotation.eulerAngles.z;
      Vector3 rocketToPlayer = GameManager.Instance.player.transform.position - transform.position;
      targetAngle = NormaliseAngle(Mathf.Atan2(rocketToPlayer.y, rocketToPlayer.x) * Mathf.Rad2Deg - 90.0f); //-90 accounts for sprite forward being off 90 degrees.
      totalTime = (Mathf.Abs(startingAngle - targetAngle) / SPIN_DEGREES) / spinSpeed;
      yield return StartCoroutine(Turn(startingAngle, targetAngle, totalTime));

      //Fire at the player
      animator.SetTrigger("Fire");
      PlayClip(fireAudio);
      while (true) {
         transform.Translate(Vector2.up * firingSpeed * Time.deltaTime);
         yield return null;
      }
   }

   private float NormaliseAngle(float angle) {
      return (angle + SPIN_DEGREES) % SPIN_DEGREES;
   }

   //Smoothly turns the boss between the two given angles, over period seconds.
   //Does not attempt to loop around past 360 degrees, so angles provided should account for that.
   //(This allows for complete freedom in the rotations used, e.g. can go from 0 to 360.)
   private IEnumerator Turn(float startAngle, float endAngle, float period) {
      float currentAngle = startAngle;
      for (float timer = 0.0f; Mathf.Abs(currentAngle - endAngle) > float.Epsilon; timer += Time.deltaTime) {
         currentAngle = Mathf.Lerp(startAngle, endAngle, timer / period);
         transform.rotation = Quaternion.Euler(0.0f, 0.0f, currentAngle);
         yield return null;
      }
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
