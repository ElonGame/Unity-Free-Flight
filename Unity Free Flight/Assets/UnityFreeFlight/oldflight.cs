using UnityEngine;
using System.Collections;

public class oldflight
: MonoBehaviour {
	
//GUI buttons
private bool toggleStatsMenu = true;
private bool togglePhysicsMenu = true;
private bool toggleGravity = true;
private bool toggleLift = false;
private bool toggleDrag = true;

	
	
//angle at which flying body contacts an air mass
//(A plane/bird has a high angle of attack when they land, nose up, into the wind)
public float angleOfAttack;
public float liftCoefficient = 1.0f;
//Constant speed with which we'll rotate. Doesn't have effect on physics.
public float RotationSpeed = 200.0f;
	
	
	
//FORCES	
//The computed force of lift our flying body generates.
public float liftForce;
//The speed of our flying body
public float AmbientSpeed = 20.0f;
//The drag againsnt our flying body
public float dragForce;
//Gravity currently set by Physics.gravity
//public float gravity = 0.2f;
public float LiftInducedDrag;
public float formDrag;
	
	
//FLYING BODY SPECIFICATIONS
public float wingChord; //in meters
public float wingSpan;  //in meters
public float weight;	// in kilograms
//generated vars
public float wingArea; // span * chord
public float aspectRatio; //span / chord
public float liftToWeightRatio; // will be important, not using it now.
//End flying body statistics
	
	
public Vector3 TESTVELOCITY;
public float TESTMAGNITUDE;
public Vector3 moveRotation;
public Vector3 anglularVelocity;
public Vector3 moveDirection = Vector3.forward;
public Vector3 vdrag = Vector3.back;
public Vector3 vlift = Vector3.up;

public Vector3 userRotationInput;
public Quaternion curRotation;
public Vector3 curVelocity;
public Quaternion newRotation;
public Vector3 newVelocity;
	
void Start() {
	rigidbody.velocity = new Vector3(1.0f, 0.0f, 10.0f);
	rigidbody.freezeRotation = true;
}
	
void Update() {
	
	//Pitch
	userRotationInput.x = Input.GetAxis("Vertical") * (RotationSpeed * Time.deltaTime);
    //Roll
	userRotationInput.z = -Input.GetAxis("Horizontal") * (RotationSpeed * Time.deltaTime);
    //Yaw
	userRotationInput.y = Input.GetAxis("Yaw") * (RotationSpeed * Time.deltaTime);	
}
	
void FixedUpdate() {
		
	newRotation = rigidbody.rotation; //Quaternion.identity;	
	newVelocity = rigidbody.velocity;
	//curRotation = rigidbody.rotation;
	//curVelocity = rigidbody.velocity;
	

	newRotation *= applyUserInput(userRotationInput);
		
	newRotation = getNewYawAndRoll(newRotation);
		
	newVelocity = getDirectionalVelocity(newRotation, newVelocity);	
	
	angleOfAttack = getAngleOfAttack(newRotation, newVelocity);	
	
		
	rigidbody.rotation = newRotation;
	rigidbody.velocity = newVelocity;	
		
		
    //rigidbody.AddForce(AddPos * (Time.deltaTime * AmbientSpeed));	
	//AmbientSpeed *= .8f;
	//lift = 1/2 V^2 * Lc * P * A
	//p = pressure; A = wing area
	//P at 10,000 feet is .45817 kg/m^3
	//
	//Example:
	//Plane flying at 205.77meters/second weighing 233531.64 newtons and having a wing area of 105.41 meters
	//liftForce = AmbientSpeed * AmbientSpeed / 2 * liftCoefficient; 
	
	//make wing dimensions
	iAmATurkeyVulture();
	wingArea = wingSpan * wingChord;
	aspectRatio = wingSpan / wingChord;
		
	moveRotation = rigidbody.rotation.eulerAngles;
	anglularVelocity = rigidbody.angularVelocity;
		
	liftCoefficient = getLiftCoefficient(angleOfAttack);
	
	//apply lift force
	liftForce = getLift(newVelocity.magnitude - newVelocity.y, 0, wingArea, liftCoefficient) * Time.deltaTime;
	vlift =  (Vector3.up * liftForce);
	if (toggleLift) {
		rigidbody.AddForce(vlift, ForceMode.Force);
	}
		
	//get drag rotation
	dragForce = getDrag(liftForce, 0, newVelocity.magnitude, wingArea, aspectRatio) * Time.deltaTime;
	vdrag = (-Vector3.forward * dragForce);
	if (toggleDrag) {
		rigidbody.AddForce (vdrag);
	}
		
	
	//MAX FORCE CONSTRAINT
	if(rigidbody.velocity.magnitude > 100) {
			Debug.Log(string.Format("----- MAX FORCE CONSTRAINT WARNING -----\n\nTime {0}\nVelocity: {1} \nRotation {2} \nMagnitude {3}\n\n-----  -----",
				Time.realtimeSinceStartup, rigidbody.velocity, rigidbody.rotation.eulerAngles, rigidbody.velocity.magnitude));
			rigidbody.velocity *= 0.9f;

		}
}

Quaternion applyUserInput(Vector3 theUserRotationInput) {
	Quaternion theNewRotation = Quaternion.identity;
	theNewRotation.eulerAngles = theUserRotationInput;
	return theNewRotation;
}
	
//Get new yaw and roll, store the value in newRotation
Quaternion getNewYawAndRoll(Quaternion theCurrentRotation) {
	Quaternion angVel = Quaternion.identity;
		
	//Apply Yaw rotations. Yaw rotation is only applied if we have angular roll. (roll is applied directly by the 
	//player)
		
	//Get the current amount of Roll, it will determine how much yaw we apply.
	float zRot = Mathf.Sin (theCurrentRotation.eulerAngles.z * Mathf.Deg2Rad) * Mathf.Rad2Deg;
	//We don't want to change the pitch in turns, so we'll preserve this value.
	float prevX = theCurrentRotation.eulerAngles.x;
	//Calculate the new rotation. The constants determine how fast we will turn.
	Vector3 rot = new Vector3(0, -zRot * 0.8f, -zRot * 0.5f) * Time.deltaTime;
		
	//Apply the new rotation 
	angVel.eulerAngles = rot;
	angVel *= theCurrentRotation;	
	angVel.eulerAngles = new Vector3(prevX, angVel.eulerAngles.y, angVel.eulerAngles.z);
		
	//Done!
	return angVel;	
}
	
Vector3 getDirectionalVelocity(Quaternion theCurrentRotation, Vector3 theCurrentVelocity) {
	//float forwardVelocity = Mathf.Abs(rigidbody.velocity.x) + Mathf.Abs(rigidbody.velocity.z);
	Vector3 vel = theCurrentVelocity;
	//float angle = theCurrentRotation.eulerAngles.y;
		
	vel = (theCurrentRotation * Vector3.forward).normalized * theCurrentVelocity.magnitude;
	//We don't want to change the y velocity
	vel.y = theCurrentVelocity.y;	
	return vel;
}
	
float getAngleOfAttack(Quaternion theCurrentRotation, Vector3 theCurrentVelocity) {
	float theAngleOfAttack;
	/*
	Vector3 velRot = Quaternion.LookRotation(theCurrentVelocity) * Vector3.forward;
	Vector3 curRot = theCurrentRotation * Vector3.forward;
	//Vector3 curRot = Vector3.forward;
		
	float velAngle = Mathf.Atan2(velRot.y, velRot.z) * Mathf.Rad2Deg;
	float curAngle = Mathf.Atan2(curRot.y, curRot.z) * Mathf.Rad2Deg;
		
	theAngleOfAttack = Mathf.DeltaAngle(velAngle, curAngle);
	//relativeWindVelocity = Quaternion.LookRotation(theCurrentVelocity).eulerAngles;
	*/
	theAngleOfAttack = - Mathf.Sin(theCurrentRotation.eulerAngles.x * Mathf.Deg2Rad) * Mathf.Rad2Deg;
	return theAngleOfAttack;
}

/*
	 Velocity -- must be in meters/second
	 pressure -- don't worry about this for now
	 area	  -- something good
	 angleOfAttack -- given in forward pitch relative to speed
	 
	 returns: 
	 
*/ 
float getLift(float velocity, float pressure, float area, float attackAngle) {
		//pressure = .45817f;
		pressure = 1.225f;
		//attackAngle = 10.0f;
		
		float lift = velocity * velocity * pressure * area * attackAngle;
		return lift;
}
	
	
float getDrag(float lift, float pressure, float velocity, float area, float aspectR) {
		//wing span efficiency value
		float VSEV = .9f;
		pressure = 1.225f;

		LiftInducedDrag = (lift*lift) / (.5f * pressure * velocity * velocity * area * Mathf.PI * VSEV * aspectR);
		formDrag = .5f * pressure * velocity * velocity * getDragCoefficient(angleOfAttack) * area;
		return LiftInducedDrag + formDrag;
}
	
float getLiftCoefficient(float angleDegrees) {
		float cof;
		if(angleDegrees > 40.0f)
			cof = 0.0f;
		if(angleDegrees < 0.0f)
			cof = angleDegrees/90.0f + 1.0f;
		else
			cof = -0.0024f * angleDegrees * angleDegrees + angleDegrees * 0.0816f + 1.0064f;
		return cof;
	}
	
float getDragCoefficient(float angleDegrees) {
		float cof;
		//if(angleDegrees < -20.0f)
		//	cof = 0.0f;
		//else
		cof = .0039f * angleDegrees * angleDegrees + .025f;
		return cof;
	}
	
	
void iAmATurkeyVulture() 	{
	wingSpan = 1.715f;
	wingChord = .7f;
	weight = 1.55f;
	
}
	
//testing purposes	
void OnGUI() {
		
	toggleStatsMenu = GUILayout.Toggle(toggleStatsMenu, "Show Stats");
	togglePhysicsMenu = GUILayout.Toggle(togglePhysicsMenu, "Show Physics");
	
		
	if (toggleStatsMenu) {
		GUI.Box(new Rect(310, 10, 400, 120), string.Format ("Stats:\nWing Span: {0} M\n Wing Chord: {1} M\n Total Wing Area: {2} M^2\nAspect Ratio: {3} S/C\n Weight: {4} Newtons\n Lift-to-Weight ratio: {5}",
				wingSpan,
				wingChord,
				wingArea,
				aspectRatio,
				weight,
				liftToWeightRatio
			));		
			
	}
	
	if (togglePhysicsMenu) {
		GUI.Box(new Rect(100,10,200,200), string.Format("Physics:\nspeed Vector: {0}\nSpeed: {1} Km/h\nDirection {2}\nGravity: {3}\nAltitude+-: {4}\nLift: {5}\nDrag: {6}\n\tInduced{7}\n\tForm {8}\n RigidBody Drag: {9} \nAngle Of Attack: {10}\nLift COF: {11}", 
				rigidbody.velocity,
				rigidbody.velocity.magnitude * 3600.0f / 1000.0f,
				rigidbody.rotation.eulerAngles,
				Physics.gravity.y, 
				liftForce + Physics.gravity.y, 
				liftForce,
				dragForce,
				LiftInducedDrag,
				formDrag,
				rigidbody.drag,
				angleOfAttack, 
				liftCoefficient)
			);	
		toggleLift = GUILayout.Toggle(toggleLift, "Lift Force");
		toggleDrag = GUILayout.Toggle(toggleDrag, "Drag Force");
		toggleGravity = GUILayout.Toggle(toggleGravity, "Gravity");
	}
			
}

float sinDeg(float degrees) {
	return Mathf.Sin (degrees * Mathf.Deg2Rad) * Mathf.Rad2Deg;
}
	
float cosDeg(float degrees) {
	return Mathf.Cos (degrees * Mathf.Deg2Rad) * Mathf.Rad2Deg;

}
	
	
}