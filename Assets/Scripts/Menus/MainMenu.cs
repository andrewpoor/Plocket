using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

   [SerializeField] private Button continueButton;
   [SerializeField] private Button levelSelectButton;

   void Start () {
      if(!GameManager.Instance.ContinueExists) {
         MenuManager.SetButtonActive(continueButton, false); //Deactivate the continue button.
      }
      if (GameManager.Instance.LevelDatas.Count == 0) {
         MenuManager.SetButtonActive(levelSelectButton, false); //Deactivate the level select button.
      }
   }

   public void NewGame() {
      GameManager.Instance.NewGame();
   }

   public void ContinueGame() {
      GameManager.Instance.ContinueGame();
   }

   public void QuitGame() {
      GameManager.Instance.QuitGame();
   }
}
