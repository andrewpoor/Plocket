using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacardManager : MonoBehaviour {

   [SerializeField] private GameObject canvas;

   [SerializeField] private PausePlacard pausePlacard; //Displayed when pausing the game.
   [SerializeField] private LevelEndPlacard levelEndPlacard; //Displayed when completing most levels.
   [SerializeField] private GameCompletePlacard gameCompletePlacard; //Displayed when completing the final level.
   [SerializeField] private CrashPlacard crashPlacard; //Displayed when the player crashes.

   private MonoBehaviour currentPlacard;
   
	void Start () {
      pausePlacard.gameObject.SetActive(false);
      levelEndPlacard.gameObject.SetActive(false);
      gameCompletePlacard.gameObject.SetActive(false);
      crashPlacard.gameObject.SetActive(false);

      canvas.SetActive(false);
   }

   public void DisplayPausePlacard() {
      DisplayPlacard(pausePlacard);
   }

   public void DisplayLevelEndPlacard(float timeTaken, float previousBestTime, bool newRecord) {
      DisplayPlacard(levelEndPlacard);
      levelEndPlacard.SetDisplay(TimeTakenForLevel(timeTaken), TimeTakenForLevel(previousBestTime), newRecord);
   }

   public void DisplayGameCompletePlacard(float timeTaken, float previousBestTime, bool newRecord, int numLevelsCleared) {
      DisplayPlacard(gameCompletePlacard);
      gameCompletePlacard.SetDisplay(TimeTakenForLevel(timeTaken), TimeTakenForLevel(previousBestTime), newRecord, numLevelsCleared);
   }

   public void DisplayCrashPlacard() {
      DisplayPlacard(crashPlacard);
   }

   public void HidePlacard() {
      currentPlacard.gameObject.SetActive(false);
      canvas.SetActive(false);
   }

   //Returns a formatted representation of the time taken so far in the level.
   //Format is mm:ss:xxx, where xs are milliseconds.
   public static string TimeTakenForLevel(float time) {
      if (time > 3600.0f) {
         return "59:59:999";
      }

      int minutes = Mathf.FloorToInt(time / 60.0f);
      int seconds = Mathf.FloorToInt(time % 60.0f);
      int milliseconds = Mathf.FloorToInt((time - 60.0f * minutes - seconds) * 1000.0f);

      return FormatTimeDivision(minutes, 2) + ":" + FormatTimeDivision(seconds, 2) + ":" + FormatTimeDivision(milliseconds, 3);
   }

   //Formats a given fraction of time to add 0s to the front if needed.
   //E.g. 9 seconds is two units (as its max is 59 seconds) and returns "09"
   private static string FormatTimeDivision(int timeRaw, int numUnits) {
      string zeroes = "";
      int units = 10; //start in the second column, as no need to place a zero in the first, the raw time will handle that
      for (int i = 0; i < numUnits - 1; i++) {
         if (timeRaw < units) {
            zeroes += "0";
         }
         units *= 10;
      }

      return zeroes + timeRaw;
   }

   //Disable the current placard if necessary, then show the given one and make it current.
   private void DisplayPlacard(MonoBehaviour placard) {
      canvas.SetActive(true);

      if(currentPlacard != null) {
         currentPlacard.gameObject.SetActive(false);
      }
      
      currentPlacard = placard;
      currentPlacard.gameObject.SetActive(true);
   }
}
