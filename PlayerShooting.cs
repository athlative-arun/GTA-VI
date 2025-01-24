using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 20f;
    public float fireRate = 0.5f;

    [Header("Aiming Settings")]
    public Transform aimTransform; 
    public LayerMask aimLayerMask;
    private bool isAiming = false;
    private float nextFireTime = 0f;

    private Animator animator;
    public Animation gun;
    public GameObject crosshair;
    public AudioSource gunaudio;

    [Header("Effects")]
    public ParticleSystem muzzleFlash; 
    public GameObject bloodParticlePrefab; 

    [Header("Raycasting")]
    public float raycastDistance = 100f; 
    private bool targetDetected = false;
    private Image crosshairImage;
    private GameObject currentTarget;
    public PlayerMovement player;
    public bool headshot;

    [Header("Pedestrian Detection")]
    public float alertRadius = 15f; 
    public bool onpc;
    private bool isShootButtonHeld = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        crosshairImage = crosshair.GetComponent<Image>();
    }

    void Update()
    {
        isAiming = player.isAiming;
        HandleAiming();
        HandleShooting();

        if (isAiming)
        {
            CheckForTarget(); 
        }

        if (isShootButtonHeld && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }


    void HandleAiming()
    {
        if (isAiming)
        {
            crosshair.SetActive(true);
        }
        else
        {
            crosshair.SetActive(false);
        }
    }

    void HandleShooting()
    {
        if (onpc) { 
        
        if (isAiming && Input.GetMouseButton(0) && Time.time >= nextFireTime && !IsPointerOverUI())
        {
            Shoot();
            nextFireTime = Time.time + fireRate;

            if (animator != null)
            {
                gun.Play();
            }
        }
        }
    }

    void Shoot()
    {
        if (isAiming)
        {
            
            gunaudio.Play();
            muzzleFlash.Play();

            if (targetDetected)
            {
                RaycastHit hit;
                Vector3 rayDirection = bulletSpawnPoint.transform.TransformDirection(Vector3.forward);
                Vector3 rayStart = bulletSpawnPoint.position;

                if (Physics.Raycast(rayStart, rayDirection, out hit, raycastDistance, aimLayerMask))
                {
                    if (hit.collider.CompareTag("npc") || hit.collider.CompareTag("npc_head"))
                    {
                      
                        Pedestrians pedestrian = hit.collider.GetComponentInParent<Pedestrians>();
                        if (headshot)
                        {
                            pedestrian.TakeDamage(10);
                        }
                        else
                        {
                            pedestrian.TakeDamage(1);
                        }

                        
                        if (bloodParticlePrefab != null)
                        {
                            GameObject bloodEffect = Instantiate(bloodParticlePrefab, hit.point, Quaternion.LookRotation(hit.normal));
                            Destroy(bloodEffect, 0.5f); 
                        }
                    }
                }
            }

            
            AlertNearbyPedestrians();
        }
    }

    void AlertNearbyPedestrians()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, alertRadius, aimLayerMask);

        foreach (Collider collider in hitColliders)
        {
            Pedestrians pedestrian = collider.GetComponentInParent<Pedestrians>();
            if (pedestrian != null && !pedestrian.dead)
            {
                pedestrian.OnHeardShooting(transform.position);
            }
        }
    }

    void CheckForTarget()
    {
        RaycastHit hit;
        Vector3 rayDirection = bulletSpawnPoint.transform.TransformDirection(Vector3.forward);
        Vector3 rayStart = bulletSpawnPoint.position;

        Debug.DrawRay(rayStart, rayDirection * raycastDistance, Color.red);

        if (Physics.Raycast(rayStart, rayDirection, out hit, raycastDistance, aimLayerMask))
        {
            if (hit.collider.CompareTag("npc") || hit.collider.CompareTag("npc_head"))
            {
                if (!targetDetected)
                {
                    currentTarget = hit.collider.gameObject;
                    crosshairImage.color = Color.red;
                    targetDetected = true;
                }

                if (hit.collider.CompareTag("npc_head"))
                {
                    headshot = true;
                }
            }
        }
        else
        {
            if (targetDetected)
            {
                headshot = false;
                crosshairImage.color = Color.white;
                targetDetected = false;
            }
        }
    }

    
    public void OnShootButtonDown()
    {
        isShootButtonHeld = true;
    }

    
    public void OnShootButtonUp()
    {
        isShootButtonHeld = false;
    }


    private bool IsPointerOverUI()
    {
        
        if (EventSystem.current != null)
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
        return false;
    }
}
