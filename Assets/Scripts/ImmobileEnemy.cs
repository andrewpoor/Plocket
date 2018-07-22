﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmobileEnemy : EnemyType {

   public float hoveringDisplacement; //Controls how much the enemy oscillates.
   public float hoveringSpeed; //The speed of oscillation.

   private Vector2 startingPosition;

   // Use this for initialization
   protected override void Start () {
      base.Start();

      startingPosition = transform.position;
   }

   void FixedUpdate () {
      //Float up and down slightly.
      float newY = startingPosition.y + hoveringDisplacement * Mathf.Sin (Time.fixedTime * hoveringSpeed);
      transform.position = new Vector2 (startingPosition.x, newY);
   }
}
