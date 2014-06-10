﻿using UnityEngine;
using System.Collections;

/*
 * Flight Mechanics is responsible for converting user input into 
 * manipulating the wing for various physics effects.
 * 
 * It handles the math behind flapping, flarring, diving,
 * to change how physics effects the flight object.
 * 
 * Example: In order to do a 'dive', this class reduces the
 * wing area. flight physics then calculates smaller lift forces,
 * allowing the flight object to plummet.
 */ 

/// <summary>
///Flight Mechanics is responsible for converting user input into 
///manipulating the wing for various physics effects.
/// 
///It handles the math behind flapping, flarring, and diving
///to change how physics effects the flight object. It does NOT
///directly interact with the user, ever. Instead, it's intended
///that the main free flight script pass in all user input, and
///call methods from here through Fixed Update()
/// </summary>
/// 
public class FlightMechanics : FlightPhysics {

	protected bool isFlapping = false;
	private bool wingsHaveFlappedInDownPosition = false;
	protected float currentFlapTime = 0.0f;
		
	public void execute(BaseFlightController controller) {
		base.doStandardPhysics (controller.UserInput);

		wingFold (controller.LeftWingExposure, controller.RightWingExposure);

		flap (
			controller.minimumFlapTime,
			controller.regularFlaptime,
			controller.flapStrength,
			controller.RegularFlap,
			controller.QuickFlap
		);

	}

	public FlightMechanics(Rigidbody rb) : base(rb) {}

	
	/// <summary>
	/// Try to execute a flap. A regular flap can only happen in regular intervals
	/// determined by regFlapTime. A quickFlap is faster, and can happen in minFlapTime
	/// intervals or regular flaptime intervals. If we are not flapping, and the wings are
	/// open, we are guaranteed a flap. 
	/// </summary>
	/// <param name="minFlapTime">Minimum flap time.</param>
	/// <param name="regFlapTime">Reg flap time.</param>
	/// <param name="flapStrength">Flap strength.</param>
	/// <param name="regFlap">If set to <c>true</c> reg flap.</param>
	/// <param name="quickFlap">If set to <c>true</c> quick flap.</param>
	public void flap(float minFlapTime, float regFlapTime, float flapStrength, bool regFlap, bool quickFlap) {
		currentFlapTime += Time.deltaTime;

		if (regFlap && wingsOpen ()) {
			//do stuff
			//We can only flap if we're not currently flapping, or the user triggered
			//an 'interruptFlap', which just means "We're flapping faster than the regular
			//flap speed." InterruptFlaps are usually triggered by the user mashing the flap
			//button rather than just holding it down.
			if (!isFlapping || (quickFlap && currentFlapTime > minFlapTime)) {
				isFlapping = true;
				currentFlapTime = 0.0f;
				rigidbody.AddForce (new Vector3 (0, flapStrength, 0));
			}
			
			//Here we deal with flapping at a regular interval. It may be better to use something
			//other than time.deltatime, it may give us incorrect readings
			if (isFlapping) {
				if (currentFlapTime > regFlapTime * 0.9f && !wingsHaveFlappedInDownPosition) {
					rigidbody.AddForce( new Vector3 (0, -flapStrength/4, 0));
					wingsHaveFlappedInDownPosition = true;
				} else if (currentFlapTime > regFlapTime) {
					isFlapping = false;
					wingsHaveFlappedInDownPosition = false;
				}
			}

		}
	}

	public void wingFlare() {
		return;
	}

	public void wingFold(float left, float right) {
		setWingPosition (left, right);

	}

	public void thrust(float forceNewtons) {
//		rigidbody.AddForce(new Vector3(0.0f, 0.0f, forceNewtons); 
	}


	//Get (in degrees) angle of attack for zero lift coefficient
	private float getAOAForZeroLCOF() {
		return 0.0f;
	}




}
