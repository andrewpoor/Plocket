using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmobileEnemy : EnemyType {

   [SerializeField] private float hoveringDisplacement; //Controls how much the enemy oscillates.
   [SerializeField] private float hoveringSpeed; //The speed of oscillation.

   private Vector2 startingPosition;

   // Use this for initialization
   protected override void Start () {
      base.Start();

      startingPosition = transform.position;
   }

   void FixedUpdate () {
      if (Time.timeScale > float.Epsilon) {
         //Float up and down slightly.
         float newY = startingPosition.y + hoveringDisplacement * Mathf.Sin(Time.fixedTime * hoveringSpeed);
         transform.position = new Vector2(startingPosition.x, newY);
      }
   }
}
