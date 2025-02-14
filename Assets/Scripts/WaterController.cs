using UnityEngine;

public class WaterController : MonoBehaviour
{
    public float WatterGravity = 0.5f;
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
        PlayerController cCtr = collision.GetComponent<PlayerController>();

        if (cCtr != null)
        {

        }
    }
}
