using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float speed;
    Rigidbody rb;

    void Start()
    {
        Destroy(this.gameObject, 5f);
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.TransformDirection(Vector3.up) * speed);
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(this.gameObject);
    }
}
