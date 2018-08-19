using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

   [SerializeField] private Button continueButton;

   void Start () {
      if(!GameManager.Instance.ContinueExists) {
         MenuManager.SetButtonActive(continueButton, false); //Deactivate the continue button.
      }
   }

   public void NewGame() {
      GameManager.Instance.NewGame();
   }

   public void ContinueGame() {
      GameManager.Instance.ContinueGame();
   }
}
