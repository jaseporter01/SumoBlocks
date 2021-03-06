﻿using UnityEngine;
using System.Collections;
#if UNITY_METRO
using UnityEngine.WSA;
#endif


public class GameController : MonoBehaviour {

	//public variables
	private static GameController instance;

	private GameController () {}
	
	public static GameController Instance 
	{
		get {
			if (instance == null) {
				instance = new GameController ();
			}
			return instance;
		}
	}

	//private variables
	private GameObject Player, AI;
	private PlayerController playerController;
	private AIController aiController;
	private GUIText guiText;
	private GUIText timerText;
	private float roundTimer;

	//our gamestates enumeration
	enum GameState
	{
		Start,
		Playing,
		Won,
		Lost,
	};

	//we create a variable to store the game state we are 
	//currently in and set it to the start state
	private GameState CurrentState = GameState.Start;

	// Use this for initialization
	void Start () {
		//SP = this;
		Player = GameObject.FindWithTag("Player");
		AI = GameObject.FindWithTag("AI");

		guiText = GameObject.Find("GUI Text").GetComponent<GUIText>();
		timerText = GameObject.Find("TimerText").GetComponent<GUIText>();

		playerController = Player.GetComponent<PlayerController>();
		aiController = AI.GetComponent<AIController>();

		roundTimer = 0.0f;

	}
	
	// Update is called once per frame
	void Update () 
	{
		switch(CurrentState)
		{
		case GameState.Start:

			guiText.text = "Click, Tap or Press a key to Start";

			if(Input.anyKey)
			{
				guiText.enabled = false;
				playerController.Active = true;
				aiController.Active = true;
				CurrentState = GameState.Playing;
			}
			break;
		case GameState.Playing:
			if(playerController.OutOfBounds)
			{
				CurrentState = GameState.Lost;
			}
			
			else if(aiController.OutOfBounds)
			{
				CurrentState = GameState.Won;
			}
			
			else
			{
				roundTimer += Time.deltaTime;
				timerText.text = "Round Timer: " + roundTimer.ToString("0.00");
			}
			break;
		case GameState.Won:
			guiText.enabled = true;
			guiText.text = "You Win!";
			SaveHighScore(roundTimer);
			if(Input.GetKeyDown(KeyCode.Return))
			{
				Reset();
			}
			break;
		case GameState.Lost:
			guiText.enabled = true;
			guiText.text = "You Lost!";
			if(Input.GetKeyDown(KeyCode.Return))
			{
				Reset();
			}
			break;
		}
	}

	public void backButtonPress()
	{
		guiText.enabled = true;
		guiText.text = "Back Button Pressed!";

	}

	public void paused()
	{
		Time.timeScale = 0.0f;
		guiText.enabled = true;
		guiText.text = "Paused!";
	}

	public void unpaused()
	{
		Time.timeScale = 1.0f;
		guiText.enabled = false;
	}

	void Reset()
	{
		roundTimer = 0.0f;
		playerController.OutOfBounds = false;
		aiController.OutOfBounds = false;
		Player.transform.rotation = new Quaternion(0,0,0,0);
		Player.transform.position = new Vector3(0.00375f,0.3530463f,-0.9551017f);
		playerController.Active = false;
		AI.transform.rotation = new Quaternion(0,0,0,0);
		AI.transform.position = new Vector3(0.00375f,0.3449954f,0.9198211f);
		aiController.Active = false;
		CurrentState = GameState.Start;
	}

	public void Quit()
	{
		UnityEngine.Application.Quit();
	}

	void SaveHighScore(float score)
	{	
		//create a variable to hold the current high score
		float currentHighScore;

		//check if we have a saved high score
		if(PlayerPrefs.HasKey("HighScore"))
		{
			//if we do save it to our holder variable
			currentHighScore = PlayerPrefs.GetFloat("HighScore");
		}
		//if we don't, set our holder variable to the max value 
        //for an int. We do this so that we have a current high score is
        //initialized but the value will always be higher than the players 
        //score
		else
		{
			currentHighScore = int.MaxValue;
		}

		//if our current high score is greater (lower time is better) 
		//than our score we update the saved high score
		if(currentHighScore > score)
		{
#if UNITY_METRO
			//if we are on Windows 8.1 we call our Share function
			//and update our Live Tile
			WindowsGateway.ShareHighScore();
			UpdateTile(score);
#endif
			PlayerPrefs.SetFloat("HighScore", score);	
		}
	}
	
	public float GetHighScore()
	{
		//if we have a saved high score return it
		if(PlayerPrefs.HasKey("HighScore"))
		{
			return PlayerPrefs.GetFloat("HighScore");
		}
		//if not return 0
		else 
		{
			return 0.0f;
		}
	}


#if UNITY_METRO

	//this is our function to update the games Live Tile
	public void UpdateTile(float score)
	{
		//first we create a holder variable 
		//and asign it to the default Tile 
		UnityEngine.WSA.Tile liveTile = Tile.main;
		//then we update the tile with our latest high score
		//the first three strings are for images (medium,wide,large)
		//the last string is for text to display
		//you can also pass in an XML file to describe the tile
		liveTile.Update("","","", "Best round time: " + score.ToString("0.00"));
	}
#endif
}
