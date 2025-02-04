using System.Collections;
using UnityEngine;

//The script on tunnels, controlling the sequence of the dwarf crawling through tunnels.

public class Tunnel : MonoBehaviour {
    [SerializeField] private Transform destinaton;

    public void DwarfTunnel(GameObject dwarf) {
        StartCoroutine(Tunneling(dwarf));
    }

    private IEnumerator Tunneling(GameObject dwarf) {
        Vector3 targetPos = destinaton.position;
        var script = dwarf.GetComponent<DwarfControls>();
        var sr = dwarf.GetComponent<SpriteRenderer>();
        var anim = dwarf.GetComponent<Animator>();
        var axe = dwarf.transform.Find("Axe");
        var rb = dwarf.GetComponent<Rigidbody2D>();
        axe.gameObject.SetActive(false);
        targetPos.z = dwarf.transform.position.z;
        targetPos.y += 0.1f;
        anim.SetTrigger("crawlIn");
        script.active = false;
        yield return new WaitForSeconds(1.6f);
        script.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
        sr.enabled = false;
        yield return new WaitForSeconds(0.5f);
        sr.enabled = true;
        dwarf.transform.position = targetPos;
        anim.SetTrigger("crawlOut");
        yield return new WaitForSeconds(1.6f);
        script.enabled = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        if (script.hasAxe) {
            axe.gameObject.SetActive(true);
        }
        anim.SetTrigger("idle");
        script.active = true;
        script.dwarfState = DwarfControls.DwarfState.Normal;
    }
}
