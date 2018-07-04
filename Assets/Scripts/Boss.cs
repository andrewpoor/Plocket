using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy {

   private const float SINGLE_OSCILLATION = 2.0f * Mathf.PI; //The degrees, in radians, of a full sinusoidal oscillation.

   public AudioClip dormantTheme;
   public AudioClip wakingSound;
   public AudioClip battleTheme;

   public float shakeDuration = 2.0f;
   public float shakeDisplacement = 0.1f;
   public int shakeOscillations = 4;
   public int startingPositionIndex = 1; //See diagram below.

   /* Transform positions for the boss's various states.
    * Positions are numbered as below:
    * /-----------\
    * | 0       1 |
    * |           |
    * |     4     |
    * |           |
    * | 2       3 |
    * \-----------/
    */
   private const float BOSS_X = 5.76f;
   private const float BOSS_Y = 3.84f;
   private const int NUM_POSITIONS = 5;
   private readonly Vector2 TOP_RIGHT = new Vector2(BOSS_X, BOSS_Y);
   private readonly Vector2 BOTTOM_RIGHT = new Vector2(BOSS_X, -BOSS_Y);
   private readonly Vector2 TOP_LEFT = new Vector2(-BOSS_X, BOSS_Y);
   private readonly Vector2 BOTTOM_LEFT = new Vector2(-BOSS_X, -BOSS_Y);
   private readonly Vector2 CENTRE = new Vector2(0.0f, 0.0f);

   private bool inAction = false; //When true, the boss is perfoming an action such as moving or attacking.
   private bool dormant = true; //The boss starts dormant until the player attacks it.
   private Vector2[] positions; //All possible boss positions.
   private int currentPositionIndex; //Index into the array of posible positions for the current position.

   protected override void Start () {
      GameManager.Instance.SetBackgroundMusic(dormantTheme);

      positions = new Vector2[NUM_POSITIONS] {TOP_LEFT, TOP_RIGHT, BOTTOM_LEFT, BOTTOM_RIGHT, CENTRE };
      transform.position = positions[startingPositionIndex];
      currentPositionIndex = startingPositionIndex;

      base.Start();
	}

   //When not performing an action, count down towards performing the next.
   private void Update() {
      if(!inAction) {
         //do stuff
      }
   }

   protected override void ReactToDamage() {
      //If dormant, become active and start the boss sequence, beginning with movement.
      if(dormant) {
         dormant = false;
         PlayClip(wakingSound);
         GameManager.Instance.SetBackgroundMusic(null);
         Move(true);
      }
   }

   private void Move(bool firstMovement = false) {
      inAction = true;
      animator.SetTrigger("Action");
      StartCoroutine(StartMovement(firstMovement));
   }

   private IEnumerator StartMovement(bool firstMovement) {
      Vector2 origin = transform.position;

      //First shake to indicate movement phase is beginning.
      for(float timer = 0.0f; timer < shakeDuration; timer += Time.deltaTime) {
         float xDisplacement = Mathf.Sin((SINGLE_OSCILLATION * shakeOscillations) * (timer / shakeDuration)) * shakeDisplacement;
         transform.position = new Vector2(origin.x + xDisplacement, origin.y);
         yield return null;
      }

      //When moving right at the very beginning, play the battle music at the appropriate time in the sequence.
      if(firstMovement) {
         GameManager.Instance.SetBackgroundMusic(battleTheme);
      }

      int newPositionIndex = ChooseNewPosition();
      List<Vector2> route = new List<Vector2>();

      //Plan a route. Behaviour is different if starting or ending at the centre,
      // otherwise the boss moves in a Z pattern.
      if (newPositionIndex == 4) {
         //Route to centre.
      } else if (currentPositionIndex == 4) {
         //Route from centre.
      } else {
         //Route from one corner to another, in a Z pattern.
         if(SameX(currentPositionIndex, newPositionIndex)) {

         } else if (SameY(currentPositionIndex, newPositionIndex)) {

         } else { //New position is in opposite corner.

         }
      }

      currentPositionIndex = newPositionIndex;
      EndAction();
      yield return null;
   }

   private void EndAction() {
      inAction = false;
      animator.SetTrigger("Idle");
   }

   //Returns a new position index, other than the current one.
   private int ChooseNewPosition() {
      //Get available boss positions, excluding the current location.
      int[] availableBossPositions = new int[NUM_POSITIONS - 1];
      int position = 0;
      for (int i = 0; i < availableBossPositions.Length; i++) {
         if (position == currentPositionIndex) {
            //Skip over current position
            position++;
         }
         availableBossPositions[i] = position;
         position++;
      }      

      //Randomly pick a new position from the available ones.
      return availableBossPositions[Random.Range(0, availableBossPositions.Length)];
   }

   private void PlayClip(AudioClip clip) {
      audioSource.clip = clip;
      audioSource.Play();
   }
}
