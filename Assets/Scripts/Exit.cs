using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour {

   private CircleCollider2D circleCollider;
   private Animator animator;
   private AudioSource audioSource;

   // Use this for initialization
   void Start () {
      circleCollider = GetComponent<CircleCollider2D> ();
      animator = GetComponent<Animator> ();
      audioSource = GetComponent<AudioSource> ();
      GameManager.Instance.RegisterExit (this);
   }
   
   //Close the exit portal, making it inaccessible.
   public void Close() {
      animator.SetTrigger ("Close");
      circleCollider.enabled = false;
   }

   //Open the exit portal, allowing the player to leave through it.
   public void Open() {
      animator.SetTrigger ("Open");
      circleCollider.enabled = true;
      audioSource.Play ();
   }
}
