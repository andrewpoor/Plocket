using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectMenu : MonoBehaviour {

   public Sprite[] thumbnails;

   [SerializeField] private LevelSelectTile tilePrefab;

   [SerializeField] private Button leftButton;
   [SerializeField] private Button rightButton;

   private int numPages;
   private int currentPageNumber;
   private List<LevelSelectPage> pages;
   private LevelSelectPage currentPage;

   void Start () {
      pages = new List<LevelSelectPage>();
      CreateNewPage();

      //Create pages of tiles for all of the completed levels.
      List<LevelData> levelDatas = GameManager.Instance.LevelDatas;
      for(int i = 0; i < levelDatas.Count; ++i) {
         LevelSelectTile tile = Instantiate(tilePrefab, transform);
         tile.SetTileVariables(levelDatas[i], thumbnails[levelDatas[i].sceneNumber]);

         //Try to add the tile to the current page. If it is full, create a new one.
         if(!currentPage.TryAddTile(tile)) {
            CreateNewPage();
            currentPage.TryAddTile(tile);
         }
      }

      GoToPage(0);
   }

   public void GoToPage(int pageNumber) {
      currentPage.SetActive(false);

      currentPageNumber = pageNumber;
      currentPage = pages[pageNumber];
      currentPage.SetActive(true);

      SetButtonActivity();
   }

   public void NextPage() {
      if(currentPageNumber < numPages - 1) {
         GoToPage(currentPageNumber + 1);
      } else {
         Debug.Log("Tried to go out of bounds (forward direction) in the level select menu.");
      }      
   }

   public void PreviousPage() {
      if (currentPageNumber > 0) {
         GoToPage(currentPageNumber - 1);
      } else {
         Debug.Log("Tried to go out of bounds (backward direction) in the level select menu.");
      }
   }

   private void CreateNewPage() {
      currentPage = new LevelSelectPage();
      currentPage.SetActive(false);
      pages.Add(currentPage);
   }

   //Makes the left and right buttons active or inactive as appropriate.
   private void SetButtonActivity() {
      MenuManager.SetButtonActive(leftButton, (currentPageNumber > 1));
      MenuManager.SetButtonActive(rightButton, (currentPageNumber < numPages));
   }
}

public class LevelSelectPage {

   public const int TILES_PER_PAGE = 4;

   private const float TILE_X = 220.0f;
   private const float TILE_Y = 140.0f;
   private readonly Vector2 TOP_RIGHT = new Vector2(TILE_X, TILE_Y);
   private readonly Vector2 BOTTOM_RIGHT = new Vector2(TILE_X, -TILE_Y);
   private readonly Vector2 TOP_LEFT = new Vector2(-TILE_X, TILE_Y);
   private readonly Vector2 BOTTOM_LEFT = new Vector2(-TILE_X, -TILE_Y);

   private int tileCount; //Keeps track of how many tiles have been placed on the page, to catch when it is full.
   private Vector2[] tilePositons;
   private List<LevelSelectTile> tiles;

   public LevelSelectPage() {
      tilePositons = new Vector2[TILES_PER_PAGE] { TOP_LEFT, TOP_RIGHT, BOTTOM_LEFT, BOTTOM_RIGHT };
      tiles = new List<LevelSelectTile>();
   }

   //Try to add a tile to the page. If the page is full, return false to indicate as such.
   public bool TryAddTile(LevelSelectTile tile) {
      if(tileCount >= TILES_PER_PAGE) {
         return false;
      }

      ++tileCount;
      tile.GetComponent<RectTransform>().localPosition = tilePositons[tileCount - 1];
      tiles.Add(tile);

      return true;
   }

   //Determines whether the page is displayed or not.
   public void SetActive(bool active) {
      foreach(LevelSelectTile tile in tiles) {
         tile.gameObject.SetActive(active);
      }
   }
}