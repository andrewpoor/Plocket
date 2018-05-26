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

   //Singleton
   public static GameManager Instance { get; private set; }

   //Menu UI
   public GameObject canvas;
   public Text title;
   public Text subtitle;

   public Texture2D cursorTexture; //A crosshair cursor.

   private Exit exit;
   private List<Enemy> enemies;

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
         StartCoroutine (WaitUntilLevelChange ());
      } else {
         title.text = GAME_COMPLETE_TEXT;
         subtitle.text = "Levels cleared: " + SceneManager.GetActiveScene ().buildIndex;
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
}
