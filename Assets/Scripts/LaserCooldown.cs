using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls the cooldown of the player's laser.
 * Displays a visual representation of the countdown until the 
 *   laser is ready to fire again.
 */

public class LaserCooldown : MonoBehaviour {
   public PlayerController playerControler;

   private SpriteRenderer spriteRenderer;
   private Animator animator;

   void Start() {
      spriteRenderer = GetComponent<SpriteRenderer> ();
      animator = GetComponent<Animator> ();

      //The countdown visualisation isn't displayed until the laser is fired.
      spriteRenderer.enabled = false;
   }

   public void StartCooldown() {
      spriteRenderer.enabled = true;
      animator.SetTrigger ("Cooldown"); //Event trigger at end calls EndCooldown
   }

   private void EndCooldown() {
      spriteRenderer.enabled = false;
      playerControler.RechargeLaser ();
   }
}
