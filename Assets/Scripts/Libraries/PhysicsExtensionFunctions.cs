using UnityEngine;

//Uses SUVAT equation to determine an object's displacement.
//Used for calculaing trajectory of a projectile.

public static class PhysicsExtensionFunctions {

    public static float QuadraticDisplacement(float initialVelocity, float acceleration, float time) {
        return initialVelocity * time - acceleration * time * time;
    }
}