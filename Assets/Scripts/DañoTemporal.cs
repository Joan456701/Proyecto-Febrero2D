using UnityEngine;

public class DañoTemporal : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHeallth hCtr = collision.gameObject.GetComponent<PlayerHeallth>();
        hCtr.Damage();
    }
}
