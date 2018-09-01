using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelEndPlacard : MonoBehaviour {

   private const string TIME_TAKEN_DISPLAY = "Time taken: ";
   private const string BEST_TIME_DISPLAY = "Best time: ";
   private const string PREVIOUS_BEST_TIME_DISPLAY = "Previous: ";
   
   [SerializeField] private Text timeTaken;
   [SerializeField] private Text bestTime;
   [SerializeField] private Text newBestTimeLabel; //Displayed when the player's time is a new record for that level.
   [SerializeField] private Text previousBestTime; //Displayed when the player's time is a new record for that level.

   //Setup the various texts on the placard that shows up when a level is completed. 
   public void SetDisplay(string timeTaken, string previousBestTime, bool newRecord) {
      this.timeTaken.text = TIME_TAKEN_DISPLAY + timeTaken;
      this.bestTime.text = BEST_TIME_DISPLAY + (newRecord ? timeTaken : previousBestTime);
      this.previousBestTime.text = "(" + PREVIOUS_BEST_TIME_DISPLAY + previousBestTime + ")";

      this.newBestTimeLabel.enabled = newRecord;
      this.previousBestTime.enabled = newRecord;
   }
}
