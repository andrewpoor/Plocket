using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

   private const string WIN_TEXT = "Level Complete!";
   private const string WIN_SUBTITLE = "Click to Continue";
   private const string LOSE_TEXT = "You crashed.";
   private const string LOSE_SUBTITLE = "Click to Continue";
   private const string GAME_COMPLETE_TEXT = "You have beaten the game!";
   private const string GAME_COMPLETE_SUBTITLE = "Levels cleared: ";
   private const string TIME_TAKEN_DISPLAY = "Time taken: ";
   private const string TIME_TAKEN_DISPLAY_LAST = "Time taken for this level: ";
   private const string TIME_TAKEN_HIDE = "";

   //Singleton
   public static GameManager Instance { get; private set; }

   //Menu UI
   public GameObject canvas;
   public Text title;
   public Text subtitle;
   public Text timeTaken;

   public Texture2D cursorTexture; //A crosshair cursor.

   private Exit exit;
   private List<Enemy> enemies;
   private PlayerController player;

   void Awake () {
      //Singleton.
      if (Instance == null) {
         Instance = this;
      } else if (Instance != this) {
         Destroy (gameObject);
      }

      DontDestroyOnLoad (gameObject); //Persist over scenes.
      canvas.SetActive (false);
      enemies = new List<Enemy> ();
      Cursor.SetCursor (cursorTexture, new Vector2 (cursorTexture.width / 2.0f, cursorTexture.height / 2.0f), CursorMode.Auto);
   }

   void Update() {
      //Developer shortcut
      if(Input.GetButtonDown("Jump")) {
         LevelComplete();
      }
   }

   //When a new scene is loaded, the exit registers itself with the game manager.
   //Register functions might be called in any order, so anything that depends on 
   //  multiple types of entity will need to be checked in each relevant function.
   public void RegisterExit(Exit newExit) {
      exit = newExit;

      if (enemies.Count > 0) {
         exit.Close ();
      }
   }

   //When a new scene is loaded, all enemies register themselves with the game manager.
   //Register functions might be called in any order, so anything that depends on 
   //  multiple types of entity will need to be checked in each relevant function.
   public void RegisterEnemy(Enemy newEnemy) {
      enemies.Add (newEnemy);

      if (exit != null) {
         exit.Close ();
      }
   }

   //When a new scene is loaded, the player avatar registers itself with the game manager.
   //Register functions might be called in any order, so anything that depends on 
   //  multiple types of entity will need to be checked in each relevant function.
   public void RegisterPlayer(PlayerController newPlayer) {
      player = newPlayer;
   }

   public void EnemyDestroyed(Enemy enemy) {
      enemies.Remove (enemy);

      if (enemies.Count == 0) {
         exit.Open ();
      }
   }

   //Loads the first level. Probably TEMP
   public void StartGame() {
      SceneManager.LoadScene (1);
      SetupNewLevel ();
   }

   //Brings up an appropriate UI popup, giving relevant button options to the player.
   public void LevelComplete() {
      canvas.SetActive (true);

      //Check if there's a level after this one.
      //If there is, load it on player command. Otherwise the game is complete.
      if (SceneManager.GetActiveScene ().buildIndex + 1 < SceneManager.sceneCountInBuildSettings) {
         title.text = WIN_TEXT;
         subtitle.text = WIN_SUBTITLE;
         timeTaken.text = TIME_TAKEN_DISPLAY + TimeTakenForLevel();
         StartCoroutine (WaitUntilLevelChange ());
      } else {
         title.text = GAME_COMPLETE_TEXT;
         subtitle.text = GAME_COMPLETE_SUBTITLE + SceneManager.GetActiveScene ().buildIndex;
         timeTaken.text = TIME_TAKEN_DISPLAY_LAST + TimeTakenForLevel();
         StartCoroutine (WaitUntilGameFinished ());
      }
   }

   //Wait for the player to signal that it's time to continue.
   private IEnumerator WaitUntilLevelChange() {
      while (!Input.GetButtonDown("Fire1")) {
         yield return null;
      }

      SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex + 1);
      canvas.SetActive (false);
      SetupNewLevel ();
      yield return null;
   }

   //Wait for the player to signal that it's time to continue.
   private IEnumerator WaitUntilGameFinished() {
      while (!Input.GetButtonDown("Fire1")) {
         yield return null;
      }

      Application.Quit ();
      yield return null;
   }

   //For when the player dies.
   //Brings up an appropriate UI popup, giving relevant button options to the player.
   public void GameOver() {
      canvas.SetActive (true);
      title.text = LOSE_TEXT;
      subtitle.text = LOSE_SUBTITLE;
      timeTaken.text = TIME_TAKEN_HIDE;

      StartCoroutine(WaitUntilRestart());
   }

   //Wait for the player to signal that it's time to continue.
   private IEnumerator WaitUntilRestart() {
      while (!Input.GetButtonDown("Fire1")) {
         yield return null;
      }

      SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
      canvas.SetActive (false);
      SetupNewLevel ();
      yield return null;
   }

   //Clear up all references to the previous level to prepare for the new one.
   private void SetupNewLevel() {
      enemies.Clear ();
      exit = null;
   }

   //Returns a formatted representation of the time taken so far in the level.
   //Format is mm:ss:xxx, where xs are milliseconds.
   private string TimeTakenForLevel() {
      if (player.timePlayed > 3600.0f) {
         return "59:59:999";
      }

      int minutes = Mathf.FloorToInt(player.timePlayed / 60.0f);
      int seconds = Mathf.FloorToInt(player.timePlayed % 60.0f);
      int milliseconds = Mathf.FloorToInt((player.timePlayed - 60.0f * minutes - seconds) * 1000.0f);

      return FormatTimeDivision(minutes, 2) + ":" + FormatTimeDivision(seconds, 2) + ":" + FormatTimeDivision(milliseconds, 3);
   }

   //Formats a given fraction of time to add 0s to the front if needed.
   //E.g. 9 seconds is two units (as its max is 59 seconds) and returns "09"
   private string FormatTimeDivision(int timeRaw, int numUnits) {
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
}
