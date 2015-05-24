using UnityEngine;
using System.Collections;
// hello
public class PlayerControl : MonoBehaviour {
	
	public float speed = 8f;
	public float acceleration = 23f;
	public float gravity = 20f;
	public float jumpHeight = 6f;
	public float upperBound;
	public Vector3 startPos;
	[HideInInspector]
	public Quaternion startRot = new Quaternion(0f,0f,0f,0f);
	private float currentSpeed;
	private float targetSpeed;
	private Vector2 amountToMove;
	[HideInInspector]
	public bool dead;
	[HideInInspector]
	public bool objectHit;
	private AudioSource flapSource = null;
	private AudioSource deathSource = null;
	public AudioSource pointSource = null;
	private GameController gameController = null;
	private Transform playerSpriteTr = null;
	
	//Animation
	private Animator anim = null;

	// touch input
	private int touchID = -1;
	
	
	void Start () {
		// get reference to game controller
		gameController = FindObjectOfType<GameController>();

		//dead = false;
		//dying = false;

		playerSpriteTr = GetComponentsInChildren<Transform>()[1];

		if (GetComponentInChildren<Animator>() != null) {
			anim = GetComponentInChildren<Animator>();
		}
		else {
			Debug.LogError ("Error: null");
		}

		// set up audio sources
		AudioSource[] audioSource = GetComponents<AudioSource>();
		flapSource = audioSource[0];
		deathSource = audioSource[1];
		pointSource = audioSource[2];

		// set start position to current position for resetting to start position later
		startPos = transform.position;
	}
	
	void Update () {
		if (dead) {


		} else if (objectHit) {
				amountToMove.y -= gravity * Time.deltaTime;
				Move(amountToMove * Time.deltaTime);
		} else { //Playing
			amountToMove.y -= gravity * Time.deltaTime;
			
			if (Input.GetButtonDown("Jump")) {
				Jump();
			}
			
			if (Input.touchCount > 0) {
				Touch touch = Input.GetTouch(0);
				if (touch.fingerId != touchID) {
					Jump ();
					touchID = touch.fingerId;
				}
			} else { touchID = -1; }
			
			Move(amountToMove * Time.deltaTime);
		}
	}

	void OnTriggerEnter2D(Collider2D coll) {
		if(gameController.soundOn && !dead && !objectHit) { 
			deathSource.Play (); 
		}
		
		// if hit ground, just die. if hit something else, rotate down and fall to ground then die
		if (coll.tag == "Ground") {
			dead = true;
			objectHit = false;
			anim.enabled=false;
		} else { // coll.tag == "Pipe"
			objectHit = true;
		}
	}

	public void Jump() {
		if (anim == null) {anim = GetComponent<Animator>();}
		anim.SetTrigger ("FlapTrigger");
		if ((transform.position.y + jumpHeight) > upperBound) {
			amountToMove.y = upperBound - transform.position.y;
		} else {
			amountToMove.y = jumpHeight;
		}
		if(gameController.soundOn) { flapSource.Play (); }
	}
	
	private void Move (Vector2 amountToMove) {  
		float deltaX = amountToMove.x;
		float deltaY = amountToMove.y;
		
		Vector2 finalTransform = new Vector2(deltaX, deltaY);
		transform.Translate (finalTransform);
	}

	public void PlayerActive () {
		anim.enabled=true;		
	}

	public void ResetPlayer () {
		anim.enabled = false;
		dead = false;
		objectHit = false;
		deathSource.enabled = true;
		transform.Translate (startPos-transform.position);
		playerSpriteTr.transform.rotation = startRot;

		amountToMove.y = 0f;
	}


	
}

// If you are moving an object through its Transform component 
// but you want to receive Collision/Trigger messages, you must 
// attach a Rigidbody to the object that is moving. 