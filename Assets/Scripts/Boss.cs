using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy {

   private const float SINGLE_OSCILLATION = 2.0f * Mathf.PI; //The degrees, in radians, of a full sinusoidal oscillation.

   public AudioClip dormantTheme;
   public AudioClip wakingSound;
   public AudioClip battleTheme;

   public float shakeDisplacement;
   public float shakeDuration;
   public int shakeOscillations;
   public float firstShakeDuration;
   public int firstShakeOscillations;
   public int startingPositionIndex; //See diagram below.
   public float speed;

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

         //TODO: TEMP
         if (Input.GetButtonDown("Fire2")) {
            Move();
         } 
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

      float duration = firstMovement ? firstShakeDuration : shakeDuration;
      float oscillations = firstMovement ? firstShakeOscillations : shakeOscillations;

      //First shake to indicate movement phase is beginning.
      for(float timer = 0.0f; timer < duration; timer += Time.deltaTime) {
         float xDisplacement = Mathf.Sin((SINGLE_OSCILLATION * oscillations) * (timer / duration)) * shakeDisplacement;
         transform.position = new Vector2(origin.x + xDisplacement, origin.y);
         yield return null;
      }

      //When moving right at the very beginning, play the battle music at the appropriate time in the sequence.
      if(firstMovement) {
         GameManager.Instance.SetBackgroundMusic(battleTheme);
      }

      int newPositionIndex = ChooseNewPosition();
      int verticalToStart = SameX(currentPositionIndex);
      int horizontalToStart = SameY(currentPositionIndex);
      int diagonallyOppositeStart = DiagonallyOpposite(currentPositionIndex);
      Queue<Vector2> route = new Queue<Vector2>();

      //Plan a route. Behaviour is different if starting or ending at the centre,
      // otherwise the boss moves in a Z pattern.
      if (newPositionIndex == 4) {
         //Route to centre.
         route.Enqueue(positions[horizontalToStart]);
         route.Enqueue(positions[diagonallyOppositeStart]);
      } else if (currentPositionIndex == 4) {
         //Route from centre.
         route.Enqueue(positions[DiagonallyOpposite(newPositionIndex)]);
         route.Enqueue(positions[SameX(newPositionIndex)]);
      } else {
         //Route from one corner to another, in a Z pattern.
         if(verticalToStart == newPositionIndex) { //Vertically aligned
            route.Enqueue(positions[horizontalToStart]);
         } else if (horizontalToStart == newPositionIndex) { //Horizontally aligned
            route.Enqueue(positions[diagonallyOppositeStart]);
         } else { //New position is in opposite corner.
            route.Enqueue(positions[horizontalToStart]);
            route.Enqueue(positions[verticalToStart]);
         }
      }
      route.Enqueue(positions[newPositionIndex]);

      while(route.Count > 0) {
         Vector3 target = route.Dequeue();

         //While the object is some distance away from the end point, move towards it
         while ((transform.position - target).sqrMagnitude > float.Epsilon) {
            Vector2 newPosition = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
            transform.position = newPosition;
            yield return null;
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
      bool atCentre = currentPositionIndex == 4;

      //Get available boss positions, excluding the current location.
      int[] availableBossPositions = new int[NUM_POSITIONS - (atCentre ? 1 : 0)];
      int position = 0;
      for (int i = 0; i < availableBossPositions.Length; i++) {
         if (position == currentPositionIndex) {
            //Skip over current position
            position++;
         }
         availableBossPositions[i] = position;
         position++;
      }
      
      //Increase chances of moving to centre due to unequal weighting.
      //(There are more options around edges so centre is less likely by default).
      if(!atCentre) {
         availableBossPositions[availableBossPositions.Length - 1] = 4;
      }

      //Randomly pick a new position from the available ones.
      return availableBossPositions[Random.Range(0, availableBossPositions.Length)];
   }

   //Returns the position index that has the same x value as the input.
   private int SameX(int positionIndex) {
      switch(positionIndex) {
         case 0: return 2;
         case 1: return 3;
         case 2: return 0;
         case 3: return 1;
         default: return -1;
      }
   }

   //Returns the position index that has the same y value as the input.
   private int SameY(int positionIndex) {
      switch (positionIndex) {
         case 0: return 1;
         case 1: return 0;
         case 2: return 3;
         case 3: return 2;
         default: return -1;
      }
   }

   //Returns the position index that is diagonally opposite the input.
   private int DiagonallyOpposite(int positionIndex) {
      switch (positionIndex) {
         case 0: return 3;
         case 1: return 2;
         case 2: return 1;
         case 3: return 0;
         default: return -1;
      }
   }

   private void PlayClip(AudioClip clip) {
      audioSource.clip = clip;
      audioSource.Play();
   }
}
