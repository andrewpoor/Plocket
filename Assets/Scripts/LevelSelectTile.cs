using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectTile : MonoBehaviour {

   private const string COMPLETION_TIME_PREFIX = "Completion Time: ";

   [SerializeField] private Image thumbnail;
   [SerializeField] private Text levelTitle;
   [SerializeField] private Text completionTime;

   private int levelSceneNumber;

   public void SetTileVariables(LevelData data, Sprite newThumbnail) {
      thumbnail.GetComponent<Image>().sprite = newThumbnail;
      levelTitle.text = data.sceneName;
      completionTime.text = COMPLETION_TIME_PREFIX + GameManager.TimeTakenForLevel(data.completionTimeRaw);
      levelSceneNumber = data.sceneNumber;
   }

   public void LoadLevel() {
      GameManager.Instance.LoadLevel(levelSceneNumber, false);
   }
}
