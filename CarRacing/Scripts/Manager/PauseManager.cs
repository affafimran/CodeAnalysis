// Description : PauseManager.cs : Use to pause and Unpaused the game
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour {

	
// --> Pause full game in pause Mode
	public void PauseGame() {

// --> Find and Pause the Game_Manager
		GameObject game_Manager = GameObject.Find("Game_Manager");							

		if (game_Manager)
			game_Manager.GetComponent<Countdown> ().Pause();


// --> Find and Pause Cars
		GameObject[] arrCars = GameObject.FindGameObjectsWithTag("Car");							

		foreach (GameObject car in arrCars) {
			if(car.GetComponent<CarController>())car.GetComponent<CarController>().Pause ();
			if(car.GetComponent<CarAI>())car.GetComponent<CarAI>().Pause ();
		}

// --> Find and Pause Cars Camera
		GameObject[] arrCarsCamera = GameObject.FindGameObjectsWithTag("CarCamera");							

		foreach (GameObject carsCamera in arrCarsCamera) {
			carsCamera.GetComponent<Cam_Follow>().Pause ();
		}

// --> Find and Pause Particle System gameObjects
		GameObject[] arrParticle = GameObject.FindGameObjectsWithTag("ParticleSystem");							

		foreach (GameObject particle in arrParticle) {
			if(particle.GetComponent<ParticleSystem>().isPlaying)particle.GetComponent<ParticleSystem>().Pause();
			else particle.GetComponent<ParticleSystem>().Play();
		}

// --> Find and Pause the Lap Counter 
		GameObject StartLine_lapCounter = GameObject.Find("StartLine_lapCounter");							

		if (StartLine_lapCounter)
			StartLine_lapCounter.GetComponent<LapCounter> ().Pause();

	}
	// --> Find and Pause 		: 		Animations
}
