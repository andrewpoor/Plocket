using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausePlacard : MonoBehaviour {

   private const string AUDIO_BUTTON_ON = "Music:\nON";
   private const string AUDIO_BUTTON_OFF = "Music:\nOFF";

   [SerializeField] private Text audioToggleButtonText;

   private bool audioOn = true;

   private void Start() {
      audioToggleButtonText.text = AUDIO_BUTTON_ON;
   }

   public void ToggleAudio() {
      GameManager.Instance.ToggleBackgroundMusic();
      audioOn = !audioOn;
      audioToggleButtonText.text = audioOn ? AUDIO_BUTTON_ON : AUDIO_BUTTON_OFF;
   }
}
