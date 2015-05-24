using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	private GameObject playerObj = null;
	private PlayerControl playerControl = null;
	private Animator ground;

	public enum GameStates {Menu, Playing, ObjectHit, Dead};
	public GameStates GameState;

	public GameObject PipePair;
	public float pipeInterval = 1.5f;
	private float timeToNextPipe = 0;
	//public bool gameRunning = false;
	//public bool bNewGame = true;
	private float dyingTime = 2.0f;
	public bool soundOn = true;

	//backdrop related items
	private Animator backdrop = null;

	//Stats
	public int points = 0;
	private bool pointsChanged = true;
	private int bestScore = 0;

	//GUI
	public GUISkin Skin = null;
	public Texture2D MainTitle = null;
	private float MainTitleScale = .7f;
	private Rect MainTitleRect;
	public Texture2D StartButton = null;
	private float StartButtonScale = .4f;
	private Rect StartButtonRect;
	public Texture2D SoundOn = null;
	public Texture2D SoundOff = null;
	private float SoundToggleScale = .1f;
	private Rect SoundToggleRect;
	private float PointsScale = .1f;
	private Rect PointsRect;
	private float BestScoreScale = .09f;
	private Rect BestScoreRect;
	private float scrW;
	private float scrH;
	private float w;
	private float h;
	private float asp;
	//private string touchXY = "no touch yet";


	// Use this for initialization
	void Start () {
		// get references
		playerObj = GameObject.Find("Player");
		playerControl = playerObj.GetComponent<PlayerControl>();
		ground = GameObject.Find("Ground").GetComponentInChildren<Animator>();
		backdrop = GameObject.Find ("Backdrop").GetComponentInChildren<Animator>();

		// retrieve high score if any
		// move this to SetGameState under Menu?
		bestScore = PlayerPrefs.GetInt("BestScore");

		// get shorthand for screen width and height
		scrW = Screen.width;
		scrH = Screen.height;

		// set numbers for MainTitle
		asp = MainTitle.width/MainTitle.height;
		w = scrW * MainTitleScale;
		h = w / asp;

		// x = screen center - half of scaled width of TEXTURE
		// y = 20% of the way down the screen
		// width = 60% of texture width
		// height = maintain aspect ratio
		MainTitleRect = new Rect( scrW/2-w/2, scrH*.2f, w, h );

		// set numbers for StartButton
		asp = StartButton.width/StartButton.height;
		w = scrW * StartButtonScale;
		h = w / asp;
		
		StartButtonRect = new Rect( scrW/2-w/2, scrH*.6f, w, h );

		// set numbers for SoundToggle
		asp = SoundOn.width/SoundOn.height;
		w = scrW * SoundToggleScale;
		h = w / asp;
		
		SoundToggleRect = new Rect( scrW-w-10, 10, w, h );


		// set numbers for Points
		asp = .3f;
		Skin.label.fontSize = (int) (scrW * PointsScale);
		w = Skin.label.fontSize;
		h = w / asp;

		PointsRect = new Rect( scrW/2-w/2, scrH * .01f, w*2f, h);

		// set numbers for High Score
		asp = .8f;
		Skin.label.fontSize = (int) (scrW * BestScoreScale);
		w = Skin.label.fontSize;
		h = w / asp;
		
		BestScoreRect = new Rect( 20f, scrH - (h*1.9f), w*7, h*2);

		// Initialize Game State
		GameState = GameStates.Dead; //So that first call will trigger

		//Reset Game
		SetGameState(GameStates.Menu);
		

	}

	public void SetGameState (GameStates newState) {
		if (GameState != newState) {
			GameState = newState;

			if (GameState == GameStates.Menu) {
				//print("Changed to Menu mode");

				playerControl.enabled = false;
				ground.enabled = false;
				backdrop.enabled = false;
				
				points = 0;
				
				// Destroy all PipePairs in scene
				GameObject[] PipesToDestroy;
				PipesToDestroy = GameObject.FindGameObjectsWithTag("Pipe");
				for (int i = 0; i < PipesToDestroy.Length; i++) {
					Destroy(PipesToDestroy[i]);
				}
				timeToNextPipe = pipeInterval;
				
				playerControl.ResetPlayer ();
				StopCoroutine("DyingDelay");
			}
			else if (newState == GameStates.Playing) {
				//print("Changed to Playing  mode");

				playerControl.enabled = true;
				ground.enabled = true;
				backdrop.enabled = true;
				playerControl.PlayerActive();
			}
			else if (newState == GameStates.ObjectHit) {
				//print("Changed to ObjectHit mode");

				ground.enabled = false;
				backdrop.enabled = false;
				PlayerPrefs.Save ();
			}
			else if (newState == GameStates.Dead) {
				//print("Changed to Dead mode");

				ground.enabled = false;
				backdrop.enabled = false;
				PlayerPrefs.Save ();
				StartCoroutine ("DyingDelay");
			}
		}
	}

	public IEnumerator DyingDelay() {
		yield return new WaitForSeconds(dyingTime);
		SetGameState(GameStates.Menu);
	}	


	// Update is called once per frame
	void Update () {
		//Check for state: 1. New Game 2. Player died 3. Game in progress

		if (GameState == GameStates.Menu) { //if (bNewGame) {
			// wait for input (jump button or pressing the start button on screen)
			if (Input.GetButtonDown("Jump")) {
				SetGameState(GameStates.Playing);
				playerControl.Jump();
			}
			if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began) {
				Touch touch = Input.GetTouch(0);

				int convertedY = Screen.height - (int) touch.position.y;
				
				// start button handling
				if (StartButtonRect.Contains(new Vector2(touch.position.x, convertedY))) { 
					SetGameState(GameStates.Playing);
					playerControl.Jump();
				}

				// sound toggle code
				if(SoundToggleRect.Contains(new Vector2(touch.position.x, convertedY))) { 
					soundOn = !soundOn; 
				}
			}
		} else if (playerControl.dead) {
			SetGameState(GameStates.Dead);
		} else if (playerControl.objectHit) {
			SetGameState(GameStates.ObjectHit);
		} else {
			SetGameState (GameStates.Playing);
			// game in progress
			
			if(timeToNextPipe < 0) {
				timeToNextPipe = pipeInterval;
				Instantiate(PipePair);
			} else {
				timeToNextPipe -= Time.deltaTime;
			}
		}
	}

	void OnGUI() {
		GUI.skin = Skin;
		
		if (GameState == GameStates.Menu) { //if(bNewGame) {
			GUI.DrawTexture(MainTitleRect, MainTitle);
			GUI.DrawTexture(StartButtonRect, StartButton);
			
			// sound toggle info
			if (soundOn) { 
				GUI.DrawTexture(SoundToggleRect, SoundOn);
			} else {
				GUI.DrawTexture(SoundToggleRect, SoundOff);
			}
			
		} else if (playerControl.dead) {
			GUI.Label (BestScoreRect, "Score: " + points.ToString () + "\nBest: " + bestScore.ToString());
		} else {
			if (pointsChanged) { 
				GUI.Label (PointsRect, points.ToString ());
			}
		}
	}
	
	public void AddPoints (int pointsToAdd) {
		pointsChanged = true;
		points += pointsToAdd;

		if(soundOn) { 
			playerControl.pointSource.Play (); 
		}

		if (points > bestScore) {
			bestScore = points;
			PlayerPrefs.SetInt ("BestScore", bestScore);
		}
	}

}
