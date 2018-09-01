using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

   private const float TRANSLUCENT_BUTTON_ALPHA = 0.4f;
   private const float OPAQUE_BUTTON_ALPHA = 1.0f;

   [SerializeField] private GameObject mainMenu;
   [SerializeField] private GameObject levelSelectMenu;

   private GameObject currentMenu;

   void Start() {
      levelSelectMenu.SetActive(false);
      currentMenu = mainMenu;

      //Choose which menu opens up at the beginning. This varies based on the state of the game.
      switch (GameManager.Instance.MenuLocation) {
         case GameManager.MenuType.MainMenu: {
            //Starts at main menu automatically so nothing needs to be done.
            break;
         }
         case GameManager.MenuType.LevelSelectMenu: {
            GoToMenu(levelSelectMenu);
            break;
         }
         default: {
            GoToMenu(mainMenu);
            break;
         }
      }
   }

   //Sets button interactivity, and also transparency to visually indicate this.
   public static void SetButtonActive(Button button, bool active) {
      //Make translucent
      Color newColor = button.GetComponent<Image>().color;
      newColor.a = active ? OPAQUE_BUTTON_ALPHA : TRANSLUCENT_BUTTON_ALPHA;
      button.GetComponent<Image>().color = newColor;

      button.interactable = active;
   }
   
   public void GoToMainMenu() {
      GoToMenu(mainMenu);
   }

   public void GoToLevelSelectMenu() {
      GoToMenu(levelSelectMenu);
      levelSelectMenu.GetComponent<LevelSelectMenu>().GoToPage(0);
   }

   //Deactivate the current menu and move to a new one.
   private void GoToMenu(GameObject menu) {
      currentMenu.SetActive(false);
      currentMenu = menu;
      currentMenu.SetActive(true);
   }
}