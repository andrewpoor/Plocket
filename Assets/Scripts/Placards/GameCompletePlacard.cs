using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCompletePlacard : MonoBehaviour {

   private const string TIME_TAKEN_DISPLAY = "Time taken for this level: ";
   private const string BEST_TIME_DISPLAY = "Best time: ";
   private const string PREVIOUS_BEST_TIME_DISPLAY = "Previous: ";
   private const string LEVELS_CLEARED_DISPLAY = "Levels cleared: ";

   [SerializeField] private Text timeTaken;
   [SerializeField] private Text bestTime;
   [SerializeField] private Text newBestTimeLabel; //Displayed when the player's time is a new record for that level.
   [SerializeField] private Text previousBestTime; //Displayed when the player's time is a new record for that level.
   [SerializeField] private Text levelsCleared;


   //Setup the various texts on the placard that shows up when a level is completed. 
   public void SetDisplay(string timeTaken, string previousBestTime, bool newRecord, int numLevelsCleared) {
      this.timeTaken.text = TIME_TAKEN_DISPLAY + timeTaken;
      this.bestTime.text = BEST_TIME_DISPLAY + (newRecord ? timeTaken : previousBestTime);
      this.previousBestTime.text = "(" + PREVIOUS_BEST_TIME_DISPLAY + previousBestTime + ")";
      this.levelsCleared.text = LEVELS_CLEARED_DISPLAY + numLevelsCleared;

      this.newBestTimeLabel.enabled = newRecord;
      this.previousBestTime.enabled = newRecord;
   }
}
