using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public enum TileType
	{
		GRASS,
		ICE,
		ICE_MOUNTAIN,
		DRY,
		DRY2,
		WATER,
		WATER_PLANT
	}

	public enum TileSpecific
	{
		NONE,
		TREE,
		STONE,
		GEM,
		BUILDING
	}

	public enum BuildingType
	{
		CASTLE,
		LUMBER,
		CRYSTAL_MINE,
		BALLISTA_TOWER,
		CANNON_TOWER,
		WIZARD_TOWER,
		BARRACK,
		GREAT_HALL
	}

	public static GameManager _instance;

	public bool isMobile;

	public Tile[] treeTiles;

	public Tile[] gemTiles;

	public Tile currentTile;

	[HideInInspector]
	public bool isBuildingUnderPlacement;

	[HideInInspector]
	public bool isCastlePlaced;

	[HideInInspector]
	public Transform castleTransform;

	[SerializeField]
	private BuildingButton[] buildingButtons;

	public List<Building> allPlacedBuildings;

	public bool isPaused;

	[SerializeField]
	private GameObject pauseMenu;

	[SerializeField]
	private GameObject fadeOutObject;

	[SerializeField]
	private GameObject gameOverWindow;

	[SerializeField]
	private GameObject gameOverEffect;

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
		Application.targetFrameRate = 60;
		fadeOutObject.SetActive(value: true);
		isBuildingUnderPlacement = false;
		allPlacedBuildings = new List<Building>();
	}

	private void Update()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			if (!isPaused)
			{
				PauseMenu();
			}
			else
			{
				Resume();
			}
		}
	}

	public void CastlePlaced(Transform t)
	{
		castleTransform = t;
		isCastlePlaced = true;
		Tutorial._instance.OnCastleTutorialFinished();
	}

	public BuildingButton GetBuildingButton(BuildingType t)
	{
		BuildingButton[] array = buildingButtons;
		foreach (BuildingButton buildingButton in array)
		{
			if (buildingButton.type == t)
			{
				return buildingButton;
			}
		}
		return null;
	}

	public void PauseMenu()
	{
		Time.timeScale = 0f;
		pauseMenu.SetActive(value: true);
		isPaused = true;
	}

	public void BackToMenu()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(0);
	}

	public void Resume()
	{
		pauseMenu.SetActive(value: false);
		Time.timeScale = 1f;
		isPaused = false;
	}

	public void GameOver()
	{
		foreach (Enemy enemy in EnemySpawner._instance.enemies)
		{
			enemy.enabled = false;
		}
		EnemySpawner._instance.enabled = false;
		MessageHandler._instance.ShowMessage("Castle is destroyed", 1.5f, Color.red);
		gameOverWindow.SetActive(value: true);
		gameOverEffect.SetActive(value: true);
		PlayerPrefs.DeleteKey("HAS_SAVE");
	}

	public static bool IsPointerOverUIObject()
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = new Vector2(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y);
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEventData, list);
		return list.Count > 0;
	}
}
