using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

   [HideInInspector] public bool Alive { get; private set; }
   [HideInInspector] public float TimePlayed { get; private set; } //Duration the plocket has been moving for.

   [SerializeField] private GameObject laserCooldownPrefab;
   [SerializeField] private GameObject energyShotPrefab;
   [SerializeField] private AudioClip movingAudio;
   [SerializeField] private AudioClip explodingAudio;
   [SerializeField] private AudioClip warpingAudio;

   [SerializeField] private float maxSpeed; //Speed of plocket once it has fully accelerated.
   [SerializeField] private float accelerationTime; //Time in seconds to reach max speed from a standstill.
   [SerializeField] private float rotationSpeed;
   [SerializeField] private float pullToExitSpeed; //Speed at which the player is pulled to the exit upon touching it.
   [SerializeField] private float shotRechargeDelay;
   [SerializeField] private float movingAudioVolume;
   [SerializeField] private LayerMask blockingLayer;
   [SerializeField] private int laserDamage;
   [SerializeField] private int shotDamage;

   private bool playerReady = false; //Used to make the rocket only move when the player is ready.
   private bool lockMovement = false; //Used by the game to allow or disallow rocket movement regardless of player input.
   private bool shotReady = false;
   private bool timing = false; //Flag for whether time should be tracked at the moment.
   private float accelerationProgress = 0.0f; //Time in seconds since acceleration begun.
   private float speed = 0.0f; //Current speed.

   #pragma warning disable 0414
   private bool laserReady = false;
   #pragma warning restore 0414

   private Animator animator;
   private PolygonCollider2D polygonCollider;
   private AudioSource audioSource;
   private LaserCooldown laserCooldown;
   private Vector2 shotOffset; //Energy shots should be fired from the front end of the rocket.

	// Use this for initialization
	void Start () {
      animator = GetComponent<Animator> ();
      polygonCollider = GetComponent<PolygonCollider2D> ();
      audioSource = GetComponent<AudioSource> ();
      laserCooldown = GameObject.Instantiate (laserCooldownPrefab, transform).GetComponent<LaserCooldown>();
      laserCooldown.playerControler = this;
      shotOffset = (GetComponent<Renderer> ().bounds.size.y / 2.0f) * Vector2.up;
      GameManager.Instance.RegisterPlayer (this);
      Alive = true;

      StartCoroutine (WaitUntilPlayerReady ());
	}

   void FixedUpdate() {
      //When moving, the rocket always moves forwards.
      //It also rotates towards the mouse position.
      if (playerReady && !lockMovement && Time.timeScale > float.Epsilon) {
//         //Rotate rocket to point towards the mouse
//         Vector3 playerToMouse = Input.mousePosition - Camera.main.WorldToScreenPoint (transform.position);
//         if (playerToMouse.magnitude > GetComponent<Renderer> ().bounds.size.y) {
//            float angle = Vector3.Angle (transform.up, playerToMouse);
//            Vector3 cross = Vector3.Cross (transform.up, playerToMouse); //Used to check the angle's sign.
//            if (cross.z < 0) {
//               angle = -angle;
//            }
//            transform.Rotate(new Vector3(0, 0, angle * rotationSpeed * Time.deltaTime));
//         }

         transform.Rotate(new Vector3(0, 0, -Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime));
         transform.Translate (Vector2.up * speed * Time.deltaTime);
      }
   }

   void LateUpdate() {
      if (timing) {
         TimePlayed += Time.deltaTime;
      }

      if(Input.GetButton("Fire1") && shotReady && !lockMovement && Time.timeScale > float.Epsilon) {
         ShootEnergyShot ();
      }
   }

   //Until the player has pressed a button to start, the rocket should remain still.
   //Once ready, the rocket will begin to accelerate and control is granted to the player.
   private IEnumerator WaitUntilPlayerReady() {
      while (!Input.GetButtonDown ("Fire1") || Time.timeScale < float.Epsilon) {
         yield return null;
      }

      playerReady = true;
      StartTimer ();
      PlayAudio (movingAudio, true, 0.0f);
      StartCoroutine (Accelerate ());
      yield return null;
   }

   //Brings the rocket up to full speed.
   private IEnumerator Accelerate() {
      while (speed < maxSpeed) {
         accelerationProgress += Time.deltaTime;
         float t = accelerationProgress / accelerationTime;
         speed = Mathf.Lerp (0.0f, maxSpeed, t);
         audioSource.volume = Mathf.Lerp (0.0f, movingAudioVolume, t);
         yield return null;
      }

      laserReady = true;
      shotReady = true;
      yield return null;
   }

   //Fire a laser in the direction the rocket is pointing.
   //Begins a cooldown during which the laser can't be fired again.
   //NOTE: Not currently in use.
   private void ShootLaser() {
      laserReady = false;
      RaycastHit2D hit = Physics2D.Raycast (transform.position, transform.up, 100.0f, blockingLayer);
      if (hit.transform != null) {
         DrawLine (transform.position, hit.point, Color.gray, 0.1f, 0.02f);

         if (hit.transform.CompareTag("Damageable")) {
            IDamageable hitObject = hit.transform.GetComponent<IDamageable>();
            hitObject.DealDamage (laserDamage);
         }
      }

      laserCooldown.StartCooldown ();
   }

   public void RechargeLaser() {
      laserReady = true;
   }

   //Draws a temporary line on screen between start and end.
   private void DrawLine(Vector2 start, Vector2 end, Color color, float duration, float width)
   {
      GameObject lineHolder = new GameObject();
      lineHolder.transform.position = start;
      lineHolder.AddComponent<LineRenderer>();
      LineRenderer line = lineHolder.GetComponent<LineRenderer>();

      line.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
      line.startColor = color;
      line.endColor = color;
      line.startWidth = width;
      line.endWidth = width;
      line.SetPosition(0, start);
      line.SetPosition(1, end);
      line.sortingLayerName = "Walls";

      GameObject.Destroy(lineHolder, duration);
   }

   //Fires a shot towards the current mouse position, independant of the rocket's orientation.
   private void ShootEnergyShot() {
      Vector2 shotOrigin = transform.position + transform.rotation * shotOffset;
      Vector2 originToMouse = Input.mousePosition - Camera.main.WorldToScreenPoint (shotOrigin); //Vector towards mouse in screen space.
      GameObject shot = Instantiate(energyShotPrefab, shotOrigin, Quaternion.FromToRotation(Vector2.up, originToMouse));
      shot.GetComponent<EnergyShot> ().damage = shotDamage;
      shotReady = false;
      Invoke ("RechargeEnergyShot", shotRechargeDelay);
   }

   private void RechargeEnergyShot() {
      shotReady = true;
   }

   void OnTriggerEnter2D(Collider2D other) {
      if (other.CompareTag ("Damageable") || other.CompareTag ("Obstacle") || other.CompareTag("EnemyAttack")) {
         BeginDestruction ();
      } else if (other.CompareTag("Exit")) {
         BeginExitReached (other.transform);
      }
   }

   //Explode and trigger a game over state.
   private void BeginDestruction() {
      lockMovement = true;
      laserReady = false;
      polygonCollider.enabled = false;
      laserCooldown.gameObject.SetActive(false);
      timing = false;
      Alive = false;
      PlayAudio (explodingAudio);
      animator.SetTrigger ("Explode"); //Event trigger at end calls EndDestruction
   }

   //Called after the exploding animation has finished.
   //Triggers the game over state.
   private void EndDestruction() {
      GameManager.Instance.GameOver ();
      gameObject.SetActive (false);
   }

//   private IEnumerator WaitUntilDestroyed() {
//      while (!FinishedExplodeAnimation || !FinishedExplodeAudio) {
//         yield return null;
//      }
//
//      gameObject.SetActive (false);
//   }
//
//   private void OnDestruction() {
//      GameManager.Instance.EnemyDestroyed (this);
//      GetComponent<Renderer> ().enabled = false;
//      FinishedExplodeAnimation = true;
//   }
//
//   private void OnExplodeAudioFinished() {
//      FinishedExplodeAudio = true;
//   }

   //Move the rocket to the exit sprite's centre, then begin the exiting animation, 
   //  before finally transitioning to the next level/screen as appropriate.
   private void BeginExitReached(Transform exitTransform) {
      lockMovement = true;
      laserReady = false;
      polygonCollider.enabled = false;
      laserCooldown.gameObject.SetActive(false);
      timing = false;
      PlayAudio (warpingAudio);
      StartCoroutine (MoveToExit(exitTransform.position));
   }

   //Smoothly pull the rocket to the exit's centre, and then continue the exit process.
   private IEnumerator MoveToExit(Vector3 exit) {
      //While the object is some distance away from the end point, move towards it
      while ((transform.position - exit).sqrMagnitude > float.Epsilon) {
         Vector2 newPosition = Vector2.MoveTowards (transform.position, exit, pullToExitSpeed * Time.deltaTime);
         transform.position = newPosition;
         yield return null;
      }

      animator.SetTrigger ("Exit"); //Event trigger at end calls EndExitReached
      yield return null;
   }

   private void EndExitReached() {
      GameManager.Instance.LevelComplete ();
      gameObject.SetActive(false);
   }

   private void StartTimer() {
      TimePlayed = 0.0f;
      timing = true;
   }

   private void PlayAudio(AudioClip clip, bool loop = false, float volume = 1.0f) {
      audioSource.clip = clip;
      audioSource.Play ();
      audioSource.loop = loop;
      audioSource.volume = volume;
   }
}
