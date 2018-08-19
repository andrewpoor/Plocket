using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectTile : MonoBehaviour {

   private const string COMPLETION_TIME_PREFIX = "Completion Time: ";
   private const string COMPLETION_TIME_NIL = "";

   [SerializeField] private Image thumbnail;
   [SerializeField] private Text levelTitle;
   [SerializeField] private Text completionTime;
   [SerializeField] private Button levelSelectButton;

   private int levelSceneNumber;

   public void SetTileVariables(LevelData data, Sprite newThumbnail) {
      thumbnail.GetComponent<Image>().sprite = newThumbnail;
      levelTitle.text = data.sceneName;
      completionTime.text = COMPLETION_TIME_PREFIX + GameManager.TimeTakenForLevel(data.completionTimeRaw);
      levelSceneNumber = data.sceneNumber;
      levelSelectButton.enabled = true;
   }

   public void SetAsNonCompletedLevel(Sprite newThumbnail) {
      thumbnail.GetComponent<Image>().sprite = newThumbnail;
      levelTitle.text = GameManager.UNKNOWN_LEVEL_TITLE;
      completionTime.text = COMPLETION_TIME_NIL;
      levelSelectButton.enabled = false;
   }

   public void LoadLevel() {
      GameManager.Instance.LoadLevel(levelSceneNumber, false);
   }
}
