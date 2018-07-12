﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy {

   private const float SINGLE_OSCILLATION = 2.0f * Mathf.PI; //The degrees, in radians, of a full sinusoidal oscillation.

   public Enemy summonedEnemyPrefab;
   public Rocket rocketPrefab;
   public GameObject laserPrefab;
   public AudioClip dormantTheme;
   public AudioClip wakingSound;
   public AudioClip battleTheme;

   public float shakeDisplacement;
   public float shakeDuration;
   public int shakeOscillations;
   public float firstShakeDuration;
   public int firstShakeOscillations;

   public float timeBetweenRockets;
   public float rocketDisplacementX;
   public float rocketDisplacementY;

   public float laserTurnAngleCorner;
   public float laserDurationCorner;
   public float laserTurnAngleCentre;
   public float laserDurationCentre;
   public float laserCooldownDuration;
   public float laserTurnBackDuration;

   public int startingPositionIndex; //See diagram below.
   public float speed;
   public float timeBetweenActions;
   public float timerVariance; //The time between actions is randomised by this amount after each action.

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
   private const float SPAWNER_OFFSET_X = 0.63f;
   private const float SPAWNER_OFFSET_Y = -0.09f;
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
   private float actionTimer = 0.0f; //Counts up towards the next action usage.

   protected override void Start () {
      GameManager.Instance.SetBackgroundMusic(dormantTheme);

      positions = new Vector2[NUM_POSITIONS] {TOP_LEFT, TOP_RIGHT, BOTTOM_LEFT, BOTTOM_RIGHT, CENTRE };
      transform.position = positions[startingPositionIndex];
      currentPositionIndex = startingPositionIndex;

      base.Start();
   }

   //When not performing an action, count down towards performing the next.
   private void Update() {
      if(!inAction && !dormant) {
         actionTimer += Time.deltaTime;

         if(actionTimer >= timeBetweenActions) {
            actionTimer = 0;
            timeBetweenActions = Random.Range(timeBetweenActions - timerVariance, timeBetweenActions + timerVariance);
            StartCoroutine(PerformAction());
         }
      }

      //TODO: TMP
      if(Input.GetButtonDown("Fire2")) {
         StartCoroutine(FireLaser());
      }
   }

   protected override void ReactToDamage() {
      //If dormant, become active and start the boss sequence.
      if(dormant) {
         dormant = false;
         PlayClip(wakingSound);
         GameManager.Instance.SetBackgroundMusic(null);
         StartCoroutine(PerformAction(true));
      }
   }

   //Randomly choose an action to perform.
   //Actions are weighted to favour ones which haven't been chosen recently.
   //If firstAction is true, the boss sequence is just beginning and a special movement action is chosen.
   private IEnumerator PerformAction(bool firstAction = false) {
      inAction = true;
      animator.SetTrigger("Action");

      if(firstAction) {
         yield return StartCoroutine(Move(true));
      } else {
         //TODO: Temp
         yield return StartCoroutine(LaunchRockets());
      }

      inAction = false;
      animator.SetTrigger("Idle");
   }

   //The boss shakes to act as a warning, and then follows a path to a new position on the stage.
   //Stage positions are chosen from a predeterminded pool of specific ones.
   //Movement is partially deterministic - between two specific points, the route will always be the same.
   //  However the destination itself is chosen at random.
   private IEnumerator Move(bool firstMovement = false) {
      Vector2 origin = transform.position;
      float duration = firstMovement ? firstShakeDuration : shakeDuration;
      float oscillations = firstMovement ? firstShakeOscillations : shakeOscillations;

      //Start by shaking to indicate movement phase is beginning.
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

   //Summon enemies from the boss to send after the player.
   private IEnumerator SummonDrones() {
      Vector2 leftSpawner = transform.position + new Vector3(-SPAWNER_OFFSET_X, SPAWNER_OFFSET_Y);
      Vector2 rightSpawner = transform.position + new Vector3(SPAWNER_OFFSET_X, SPAWNER_OFFSET_Y);
      Enemy enemyLeft = Instantiate(summonedEnemyPrefab, leftSpawner, Quaternion.identity);
      Enemy enemyRight = Instantiate(summonedEnemyPrefab, rightSpawner, Quaternion.identity);
      enemyLeft.spawnIn = true;
      enemyRight.spawnIn = true;

      //Wait until the enemies have finished spawning.
      while(!enemyLeft.alive || !enemyRight.alive) {
         yield return null;
      }
   }

   //Launch rockets at the player.
   //Rockets eject out the top in sequence, spin in place for a short while, and then shoot directly at the player.
   private IEnumerator LaunchRockets() {
      Rocket leftRocket = Instantiate(rocketPrefab, transform.position, Quaternion.identity);
      leftRocket.initialDisplacement = new Vector2(-rocketDisplacementX, rocketDisplacementY);
      yield return new WaitForSeconds(timeBetweenRockets);

      Rocket middleRocket = Instantiate(rocketPrefab, transform.position, Quaternion.identity);
      middleRocket.initialDisplacement = new Vector2(0.0f, rocketDisplacementY);
      yield return new WaitForSeconds(timeBetweenRockets);

      Rocket rightRocket = Instantiate(rocketPrefab, transform.position, Quaternion.identity);
      rightRocket.initialDisplacement = new Vector2(rocketDisplacementX, rocketDisplacementY);
      yield return new WaitForSeconds(timeBetweenRockets);
   }

   //Fire a continuous laser in a pattern dependant on boss location.
   //The boss first charges up for a period, then fires the laser continually while turning to cover a certain amount of the screen.
   //In the top corners the boss turns to sweep part of the screen. In the centre the boss spins all the way around.
   private IEnumerator FireLaser() {
      //Charge
      animator.SetTrigger("Charge");
      yield return null;
      while (animator.GetCurrentAnimatorStateInfo(0).IsName("BossChargingLaser")) {
         yield return null;
      }

      //Fire laser
      GameObject laser = Instantiate(laserPrefab, transform);
      laser.transform.position = new Vector2(transform.position.x, transform.position.y - laser.GetComponent<Renderer>().bounds.size.y / 2.0f);

      //Set turning variables
      float turnAngle;
      float duration;
      if(currentPositionIndex == 4) {
         turnAngle = (Random.value > 0.5f) ? laserTurnAngleCentre : -laserTurnAngleCentre; //Spin clockwise or counterclockwise at random.
         duration = laserDurationCentre;
      } else {
         turnAngle = (currentPositionIndex == 0) ? laserTurnAngleCorner : -laserTurnAngleCorner; //Spin according to which corner the boss is in.
         duration = laserDurationCorner;
      }

      //Turn (while firing laser until end of turning)
      for(float timer = 0.0f; timer < duration; timer += Time.deltaTime) {
         transform.Rotate(0.0f, 0.0f, turnAngle * Time.deltaTime / duration);
         yield return null;
      }
      Destroy(laser);
      yield return new WaitForSeconds(laserCooldownDuration);

      //Turn back if at the corners
      if (currentPositionIndex != 4) {
         for (float timer = 0.0f; timer < laserTurnBackDuration; timer += Time.deltaTime) {
            transform.Rotate(0.0f, 0.0f, -turnAngle * Time.deltaTime / laserTurnBackDuration);
            yield return null;
         }
      }
   }

   private void PlayClip(AudioClip clip) {
      audioSource.clip = clip;
      audioSource.Play();
   }
}
