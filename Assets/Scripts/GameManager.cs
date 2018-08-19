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

   //Holds the type of menu that should be opened when the MainMenu scene starts
   public enum MenuType {
      MainMenu,
      LevelSelectMenu
   };

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

   [HideInInspector] public PlayerController Player { get; private set; }
   [HideInInspector] public MenuType MenuLocation { get; private set; }

   //If true, the save file has a playthrough saved that can be returned to by continuing the game.
   //Note that saved data stores information about all levels that have ever been completed by the player, 
   // even if they do not currently have a playthrough to continue from (usually because they completed the game).
   public bool ContinueExists { get { return saveData.continueGame; } }

   public List<LevelData> LevelDatas { get { return saveData.GetLevelDatasCopy(); } }

   //Level Complete Popup
   [SerializeField] private GameObject canvas;
   [SerializeField] private Text title;
   [SerializeField] private Text subtitle;
   [SerializeField] private Text timeTaken;

   [SerializeField] private Texture2D cursorTexture; //A crosshair cursor.
   [SerializeField] private AudioClip menuMusic;
   [SerializeField] private AudioClip levelMusic;
   [SerializeField] private int firstLevelSceneNumber = 1;
   [SerializeField] private int mainMenuSceneNumber = 0;

   private Exit exit;
   private List<Enemy> enemies;

   private AudioSource audioSource;
   private SaveData saveData;
   private string saveDataPath;
   private bool inPlaythrough; //True if starting a new game or continuing one, false if loading a level by itself.

   void Awake () {
      //Singleton.
      if (Instance == null) {
         Instance = this;
         DontDestroyOnLoad (gameObject); //Persist over scenes.
      } else if (Instance != this) {
         Destroy (gameObject);
         return;
      }

      canvas.SetActive(false);
      enemies = new List<Enemy>();
      Cursor.SetCursor(cursorTexture, new Vector2(cursorTexture.width / 2.0f, cursorTexture.height / 2.0f), CursorMode.Auto);
      audioSource = GetComponent<AudioSource>();
      SetBackgroundMusic(ChooseBackgroundMusic(SceneManager.GetActiveScene().buildIndex));
      saveDataPath = Application.persistentDataPath + "/saveData.dat";
      MenuLocation = MenuType.MainMenu;

      LoadGame();
   }

   void Update() {
      //Developer commands
      bool devKey = Input.GetKey(KeyCode.Keypad8);
      if(devKey && Input.GetKeyDown(KeyCode.Space)) {
         LevelComplete();
      } else if(devKey && Input.GetKeyDown(KeyCode.P)) {
         PrintSaveData();
      } else if (devKey && Input.GetKeyDown(KeyCode.D)) {
         DeleteSaveData();
      } else if (devKey && Input.GetKeyDown(KeyCode.A)) {
         ToggleBackgroundMusic();
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
      Player = newPlayer;
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

   //Start a new game from the first level
   public void NewGame() {
      LoadLevel(firstLevelSceneNumber, true);
   }

   //Continue the game from the level the player had reached in their last session
   public void ContinueGame() {
      if(ContinueExists) {
         LoadLevel(saveData.currentSceneNumber, true);
      } else {
         //Should not reach here in normal gameplay, as the option to continue game is unselectable in this instance.
         Debug.Log("Error: Trying to continue a game when there is no playthrough to continue from.");
      }
   }

   //Saves the game and displays the relevant level completion popup for the user.
   public void LevelComplete() {
      canvas.SetActive (true);
      Player.enabled = false;
      RecordLevelData();

      if(inPlaythrough) {
         //Check if there's a level after this one.
         //If there is, load it on player command. Otherwise the game is complete.
         if (SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings) {
            saveData.continueGame = true;
            saveData.currentSceneNumber = SceneManager.GetActiveScene().buildIndex + 1; //If the player were to stop here, they'd resume at the start of the next level.
            SaveGame();

            EditLevelEndPopup(WIN_TEXT, WIN_SUBTITLE, TIME_TAKEN_DISPLAY + TimeTakenForLevel(Player.TimePlayed));

            StartCoroutine(WaitUntilLevelChange());
         } else {
            saveData.continueGame = false;
            saveData.currentSceneNumber = firstLevelSceneNumber;
            SaveGame();

            EditLevelEndPopup(GAME_COMPLETE_TEXT, GAME_COMPLETE_SUBTITLE, TIME_TAKEN_DISPLAY + TimeTakenForLevel(Player.TimePlayed));

            StartCoroutine(WaitUntilReturnToMainMenu());
         }
      } else {
         SaveGame();
         EditLevelEndPopup(WIN_TEXT, WIN_SUBTITLE, TIME_TAKEN_DISPLAY + TimeTakenForLevel(Player.TimePlayed));
         StartCoroutine(WaitUntilReturnToMainMenu());
      }  
   }

   //Loads the given level.
   //If continueGame is true, this level is treated as part of the main playthrough, and upon finishing the level
   // the game will continue onto the next one. Additionally if the player exits after this level, it will be 
   // counted as the last level the player completed in the playthrough upon returning and continuing the game.
   //If continueGame is false, the level is treated as being played in isolation. It will return to leve select
   // upon completion, and will not impact the main playthrough. New records for the level will be saved, however.
   public void LoadLevel(int levelSceneNumber, bool continueGame, bool continueCurrentMusic = false) {
      SceneManager.LoadScene(levelSceneNumber);
      canvas.SetActive(false);
      SetupNewLevel();
      inPlaythrough = continueGame;
      MenuLocation = continueGame ? MenuType.MainMenu : MenuType.LevelSelectMenu;

      if(!continueCurrentMusic) {
         SetBackgroundMusic(ChooseBackgroundMusic(levelSceneNumber));
      }
   }

   //For when the player dies.
   //Brings up an appropriate UI popup, giving relevant button options to the player.
   public void GameOver() {
      canvas.SetActive(true);
      title.text = LOSE_TEXT;
      subtitle.text = LOSE_SUBTITLE;
      timeTaken.text = TIME_TAKEN_HIDE;

      StartCoroutine(WaitUntilRestart());
   }

   //Chooses the background music for the given level.
   private AudioClip ChooseBackgroundMusic(int sceneNumber) {
      switch(sceneNumber) {
         case 0: return menuMusic;
         default: return levelMusic;    
      }
   }

   //Setup the various texts on the popup that shows up when a level is completed. 
   private void EditLevelEndPopup(string title, string subtitle, string timeTaken) {
      this.title.text = title;
      this.subtitle.text = subtitle;
      this.timeTaken.text = timeTaken;
   }

   //Wait for the player to signal that it's time to continue and then advance to the next level.
   private IEnumerator WaitUntilLevelChange() {
      while (!Input.GetButtonDown("Fire1")) {
         yield return null;
      }

      LoadLevel(SceneManager.GetActiveScene().buildIndex + 1, true, true);
      yield return null;
   }

   //Wait for the player to signal that it's time to continue and then return to the main menu.
   //The MenuLocation property controls which menu in the main menu is opened upon return there.
   private IEnumerator WaitUntilReturnToMainMenu() {
      while (!Input.GetButtonDown("Fire1")) {
         yield return null;
      }

      LoadLevel(mainMenuSceneNumber, false);
      yield return null;
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

   //Record information about the currently completed level, such as completion time.
   //Relies on the game still being in the scene of the level to be saved.
   private void RecordLevelData() {
      int sceneNumber = SceneManager.GetActiveScene().buildIndex;
      LevelData levelData = new LevelData(sceneNumber, Player.TimePlayed, SceneManager.GetActiveScene().name);
      int index = sceneNumber - firstLevelSceneNumber;

      if(index < saveData.levelDatas.Count) {
         saveData.levelDatas[SceneManager.GetActiveScene().buildIndex - firstLevelSceneNumber] = levelData;
      } else {
         saveData.levelDatas.Insert(index, levelData);
      }
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

   /*
    * ************************
    * DEVELOPER COMMANDS
    * ************************
    */
    
   private void PrintSaveData() {
      int levelDataIndex = 0;
      foreach(LevelData levelData in saveData.levelDatas) {
         Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
         Debug.Log("Index: " + levelDataIndex);
         Debug.Log("sceneNumber: " + levelData.sceneNumber + "\ncompletionTime: " + levelData.completionTimeRaw + "\nsceneName: " + levelData.sceneName);
         Debug.Log("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");

         ++levelDataIndex;
      }
   }

   private void DeleteSaveData() {
      File.Delete(saveDataPath);
   }

   private void ToggleBackgroundMusic() {
      audioSource.enabled = !audioSource.isActiveAndEnabled;

      if (audioSource.isActiveAndEnabled) {
         audioSource.Play();
      }
   }
}

//Data to be saved to a file.
[Serializable]
public class SaveData {
   public bool continueGame; //If true, the player was partway through the levels and should start at the next in line when continuing the game.
   public int currentSceneNumber; //The level the player is currently at. The game starts here when continuing the game after closing it.
   public List<LevelData> levelDatas; //Various data about each level.

   public SaveData() {
      continueGame = false;
      currentSceneNumber = 0;
      levelDatas = new List<LevelData>();
   }

   //Get a copy of the level datas.
   //Use this for read-only usage of the save data.
   public List<LevelData> GetLevelDatasCopy() {
      List<LevelData> levelDatasCopy = new List<LevelData>();

      foreach(LevelData data in levelDatas) {
         levelDatasCopy.Add(new LevelData(data));
      }

      return levelDatasCopy;
   }
}

//Data about an individual level that can be saved to a file.
//If data for the level isn't present, that level has yet to be completed.
[Serializable]
public class LevelData {
   public int sceneNumber = 0; //Unity scene number for this level.
   public float completionTimeRaw = float.PositiveInfinity; //Fastest time the user has taken to complete the level.
   public string sceneName = GameManager.UNKNOWN_LEVEL_TITLE;

   public LevelData(int sceneNumber, float completionTimeRaw, string sceneName) {
      this.sceneNumber = sceneNumber;
      this.completionTimeRaw = completionTimeRaw;
      this.sceneName = sceneName;
   }

   public LevelData(LevelData original) {
      this.sceneNumber = original.sceneNumber;
      this.completionTimeRaw = original.completionTimeRaw;
      this.sceneName = original.sceneName;
   }
}
