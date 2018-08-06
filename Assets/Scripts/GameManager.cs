using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

   public const string UNKNOWN_LEVEL_TITLE = "???";

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
   public AudioClip menuMusic;
   public AudioClip levelMusic;
   public PlayerController player { get; private set; }
   public int firstLevelSceneNumber = 1;

   private Exit exit;
   private List<Enemy> enemies;

   private AudioSource audioSource;
   private SaveData saveData;
   private string saveDataPath;

   void Awake () {
      //Singleton.
      if (Instance == null) {
         Instance = this;
         DontDestroyOnLoad (gameObject); //Persist over scenes.
      } else if (Instance != this) {
         Destroy (gameObject);
      }

      canvas.SetActive(false);
      enemies = new List<Enemy>();
      Cursor.SetCursor(cursorTexture, new Vector2(cursorTexture.width / 2.0f, cursorTexture.height / 2.0f), CursorMode.Auto);
      audioSource = GetComponent<AudioSource>();
      SetBackgroundMusic((SceneManager.GetActiveScene().buildIndex == 0) ? menuMusic : levelMusic); //TODO: Temp
      saveDataPath = Application.persistentDataPath + "/saveData.dat";

      LoadGame();
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

   public void SetBackgroundMusic(AudioClip clip) {
      audioSource.clip = clip;
      audioSource.Play();
   }

   //Loads the first level. Probably TEMP until proper menus in place.
   public void StartGame() {
      SceneManager.LoadScene (firstLevelSceneNumber);
      SetupNewLevel ();
      SetBackgroundMusic(levelMusic); //TODO: Temp
   }

   //Saves the game and displays the relevant level completion popup for the user.
   public void LevelComplete() {
      canvas.SetActive (true);
      player.enabled = false;
      SaveLevelRecord();

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

   //Save information about the currently completed level.
   //Relies on the game still being in the scene of the level to be saved.
   private void SaveLevelRecord() {
      int sceneNumber = SceneManager.GetActiveScene().buildIndex;
      LevelData levelData = new LevelData(sceneNumber, player.timePlayed, SceneManager.GetActiveScene().name);

      int index = sceneNumber - firstLevelSceneNumber;
      if(index < saveData.levelDatas.Count) {
         saveData.levelDatas[SceneManager.GetActiveScene().buildIndex - firstLevelSceneNumber] = levelData;
      } else {
         saveData.levelDatas.Insert(index, levelData);
      }

      SaveGame();
   }

   //Saves recorded data to a file. Does not add to recorded data however.
   //To be called after all desired data has been recorded.
   private void SaveGame() {
      BinaryFormatter formatter = new BinaryFormatter();
      FileStream file = File.Create(saveDataPath);
      formatter.Serialize(file, saveData);
      file.Close();
   }

   private void LoadGame() {
      if(File.Exists(saveDataPath)) {
         BinaryFormatter formatter = new BinaryFormatter();
         FileStream file = File.Open(saveDataPath, FileMode.Open);
         saveData = (SaveData)formatter.Deserialize(file);
         file.Close();
      } else {
         saveData = new SaveData();
      }
   }
}

//Data to be saved to a file.
[Serializable]
class SaveData {
   public List<LevelData> levelDatas;

   public SaveData() {
      levelDatas = new List<LevelData>();
   }
}

//Data about an individual level that can be saved to a file.
//If data for the level isn't present, that level has yet to be completed.
[Serializable]
class LevelData {
   public int sceneNumber = 0; //Unity scene number for this level.
   public float completionTimeRaw = float.PositiveInfinity; //Fastest time the user has taken to complete the level.
   public string sceneName = GameManager.UNKNOWN_LEVEL_TITLE;

   public LevelData(int sceneNumber, float completionTimeRaw, string sceneName) {
      this.sceneNumber = sceneNumber;
      this.completionTimeRaw = completionTimeRaw;
      this.sceneName = sceneName;
   }
}
