using UnityEngine;
using System.Collections;

public class PipeControl : MonoBehaviour {

	private float height;
	public float minHeight = 4f;	// make these resolution independent
	public float maxHeight = 9.4f;
	public float speed = -2.5f;
	public float offScreenX = -20f;
	private Vector3 startPos;
	private GameObject player;
//	private PlayerControl playerControl;
	private GameController gameController;
	private bool passed = false;

	// Use this for initialization
	void Start () {
		height = Random.Range(minHeight,maxHeight);

		startPos = new Vector3(10f,height,0f);

		transform.position = startPos;
		player = GameObject.Find("Player");
		//playerControl = player.GetComponent<PlayerControl>();	
		//gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
		gameController = FindObjectOfType<GameController>();

	}


	void Update () {
		if (gameController.GameState == GameController.GameStates.Playing) {// if (!playerControl.dead) {
			transform.Translate (speed*Time.deltaTime,0f,0f);
		}
		if (!passed && transform.position.x < player.transform.position.x) {
			passed = true;
			gameController.SendMessage ("AddPoints", 1);
		}
		if (transform.position.x < offScreenX) {
			Destroy (gameObject);
		}
	}
}
