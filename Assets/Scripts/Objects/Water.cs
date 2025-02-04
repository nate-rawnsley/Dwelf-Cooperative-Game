using System.Collections;
using UnityEngine;

//Spawns particles and plays sounds when something enters the water.

public class Water : MonoBehaviour {

    private Object splashParticles;

    private void Awake() {
        splashParticles = Resources.Load("Splash");
    }

    private void OnTriggerEnter2D(Collider2D other) {
        FindCollisionPoint(other.transform);
    }

    private void OnTriggerExit2D(Collider2D other) {
        FindCollisionPoint(other.transform);
    }

    //Ensures the splash appears at the correct point, where the object that prompted it meets the water.
    private void FindCollisionPoint(Transform other) {
        RaycastHit2D[] hits = Physics2D.RaycastAll(other.position, Vector2.down, 5);
        foreach (var hit in hits) {
            if (hit.transform == transform) {
                StartCoroutine(SpawnSplashParticles(hit.point));
            }
        }
    }

    private IEnumerator SpawnSplashParticles(Vector3 position) {
        GameObject tempParticles = (GameObject)Instantiate(splashParticles, position, Quaternion.identity);
        yield return new WaitForSeconds(1);
        Destroy(tempParticles);
    }
}
