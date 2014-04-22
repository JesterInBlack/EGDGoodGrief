/* -----------------------------------------------------------------------------------------------------

    -- ParticleRingEffect --

    

    This script spawns "ring" particle effects, such as are commonly used for explosion

    shockwaves, sensor effects,  propulsion effects, etc.  Note that the script can be

    adjusted to spawn any number of ring particle systems for repeating ring effects.

    Use this feature carefully so as not to adversely impact framerate.  

    

    Assign this script to the transform at the location where the ring effect will be 

    centered.  The ring will be generated in the plane specified by the script transform's

    red axis (right) and centered around the green axis (up).  

   ------------------------------------------------------------------------------------------------------ */

 

// -- ringEffect --

// This must be set to reference the prefab that contains the particle system components.

// Those components can be adjusted as usual to achieve the desired appearance of the 

// particles.  Typical "expanding ring" effects are achieved in combination with this script by:

//  - Starting with default component settings, then

//  - Particle emitter:

//      Emit: OFF

//      Simulate in Worldspace: ON

//      One Shot: ON

//  - Particle Renderer:

//      Materials: Your favorite particle material 8^)

//  - Particle Animator:

//      Autodestruct: ON (Prevents accumulation of used-up particle systems!)

public var ringEffect : Transform;

 

// The expansion speed of the ring.

public var speed : float = 2.0;

 

// The inner and outer radii determine the width of the ring.

public var innerRadius : float = 0.5;

public var outerRadius : float = 1.5;

 

// How many ring systems to spawn.  "Infinite" by default.

public var numberOfRings : int = 9999999;

 

// How often a new ring should be spawned.  In seconds.

public var spawnRate : float = 5.0;

 

/* ------------------------------------------------------------------------------------------------------*/

// Time at which the last spawn occurred.  

private var timeOfLastSpawn : float = 0.0;

 

// Count of rings spawned so far.

private var spawnCount : int = 0;

 

 

/* ------------------------------------------------------------------------------------------------------

    -- SpawnRing --

    

    This function spawns a new particle effect system each time it's called.  The system 

    spawned is the prefab referenced by the public ringEffect variable.

   ------------------------------------------------------------------------------------------------------- */

function SpawnRing () {

    // Instantiate the effect prefab.

    var effectObject = Instantiate(ringEffect, this.transform.position, this.transform.rotation);

    

    // Parent the new effect to this script's transform.  

    effectObject.transform.parent = this.gameObject.transform;

    

    // Get the particle emitter from the new effect object.

    var emitter = effectObject.GetComponent(ParticleEmitter);

    

    // Generate the particles.

    emitter.Emit();

    

    // Extract the particles from the created emitter.  Notice that we copy the particles into a new javascript array.

    // According to the Unity docs example this shouldn't be necessary, but I couldn't get it to work otherwise.  

    // Below, when the updated p array is reassigned to the emitter particle array, the assignment failed when p was

    // simply assigned the value "emitter.particles".

    var p : Array = new Array(emitter.particles); 

    

    // Loop thru the particles, giving each an initial position and velocity.

    for (var i=0; i<p.length; i++) {

    

        // Generate a random unit vector in the plane defined by our transform's red axis centered around the 

        // transform's green axis.  This vector serves as the basis for the initial position and velocity of the particle.

        var ruv : Vector3 = RandomUnitVectorInPlane(effectObject.transform, effectObject.transform.up);

        

        // Calc the initial position of the particle accounting for the specified ring radii.  Note the use of Range

        // to get a random distance distribution within the ring.

        var newPos : Vector3 = effectObject.transform.position 

                + ((ruv * innerRadius) + (ruv * Random.Range(innerRadius, outerRadius)));

        p[i].position = newPos;

        

        // The velocity vector is simply the unit vector modified by the speed.  The velocity vector is used by the 

        // Particle Animator component to move the particles.

        p[i].velocity = ruv * speed;

    }

    // Update the actual particles.

    emitter.particles = p.ToBuiltin(Particle);

}

 

function LateUpdate() 

{

    // Check to see if it's time to spawn a new particle system.

    var timeSinceLastSpawn : float = Time.time - timeOfLastSpawn;

    if (timeSinceLastSpawn >= spawnRate && spawnCount < numberOfRings) {

        SpawnRing();

        timeOfLastSpawn = Time.time;

        spawnCount++;

    }

}

 

function RandomUnitVectorInPlane(xform : Transform, axis : Vector3) :  Vector3

{

    // Rotate the specified transform's axis thru a random angle.

    xform.Rotate(axis, Random.Range(0.0, 360.0), Space.World);

    

    // Get a copy of the rotated axis and normalize it.

    var ruv : Vector3 = new Vector3(xform.right.x, xform.right.y, xform.right.z);   

    ruv.Normalize();

    return (ruv);

}