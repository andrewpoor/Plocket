using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectMenu : MonoBehaviour {

   //References to all of the level thumbnails, in level order.
   //thumbnails[0] corresponds to the placeholder level image, for levels not yet completed.
   public Sprite[] thumbnails;

   [SerializeField] private LevelSelectTile tilePrefab;

   //Buttons for turning to the previous or next page of level tiles.
   [SerializeField] private Button leftButton;
   [SerializeField] private Button rightButton;

   private int numPages;
   private int currentPageNumber; //Indexed from 0, so goes up to (numPages - 1)
   private List<LevelSelectPage> pages; //Pages of level tiles that can be switched between.
   private LevelSelectPage currentPage;

   void Start () {
      pages = new List<LevelSelectPage>();
      CreateNewPage();

      //Create pages of tiles for all of the completed levels.
      List<LevelData> levelDatas = GameManager.Instance.LevelDatas;
      for(int i = 0; i < levelDatas.Count; ++i) {
         LevelSelectTile tile = Instantiate(tilePrefab, transform);

         if(levelDatas[i].completionTimeRaw > float.Epsilon) {
            tile.SetTileVariables(levelDatas[i], thumbnails[levelDatas[i].sceneNumber]);
         } else {
            tile.SetAsNonCompletedLevel(thumbnails[0]);
         }         

         //Try to add the tile to the current page. If it is full, create a new one.
         if(!currentPage.TryAddTile(tile)) {
            currentPage.SetInteractive(false);
            CreateNewPage();
            currentPage.TryAddTile(tile);
         }
      }

      //Fill out empty tiles to represent levels yet to complete if the current page still has space.
      int numEmptyTiles = LevelSelectPage.TILES_PER_PAGE - currentPage.TileCount;
      for (int i = 0; i < numEmptyTiles; ++i) {
         LevelSelectTile tile = Instantiate(tilePrefab, transform);
         tile.SetAsNonCompletedLevel(thumbnails[0]);
         currentPage.TryAddTile(tile);
      }

      numPages = pages.Count;
      GoToPage(0);
   }

   public void GoToPage(int pageNumber) {
      currentPage.SetInteractive(false);

      currentPageNumber = pageNumber;
      currentPage = pages[pageNumber];
      currentPage.SetInteractive(true);

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
      pages.Add(currentPage);
   }

   //Makes the left and right buttons active or inactive as appropriate.
   private void SetButtonActivity() {
      MenuManager.SetButtonActive(leftButton, (currentPageNumber > 0));
      MenuManager.SetButtonActive(rightButton, (currentPageNumber < numPages - 1));
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

   //Keeps track of how many tiles have been placed on the page, to catch when it is full.
   public int TileCount { get; private set; }

   private Vector2[] tilePositons;
   private List<LevelSelectTile> tiles;

   public LevelSelectPage() {
      TileCount = 0;
      tilePositons = new Vector2[TILES_PER_PAGE] { TOP_LEFT, TOP_RIGHT, BOTTOM_LEFT, BOTTOM_RIGHT };
      tiles = new List<LevelSelectTile>();
   }

   //Try to add a tile to the page. If the page is full, return false to indicate as such.
   public bool TryAddTile(LevelSelectTile tile) {
      if(TileCount >= TILES_PER_PAGE) {
         return false;
      }

      ++TileCount;
      tile.GetComponent<RectTransform>().localPosition = tilePositons[TileCount - 1];
      tiles.Add(tile);

      return true;
   }

   //Determines whether the page is displayed or not.
   public void SetInteractive(bool interactive) {
      foreach(LevelSelectTile tile in tiles) {
         tile.gameObject.SetActive(interactive);
      }
   }
}