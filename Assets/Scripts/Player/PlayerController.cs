using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public static PlayerController INSTANCE;

    #region Variables Públicas

    [Header ("Componentes")]
    public ShellController ShellController;
    public SpriteRenderer cRenderer;
    public Rigidbody2D cRigidbody;
    public CapsuleCollider2D cCollider;
    public PlayerHeallth cHealth;

    [Header("Configuración de Movimiento")]
    public float Speed = 5f;
    public float SpeedBase = 5f;
    public float SpeedAugmented = 8f;
    public float swimSpeed = 6f;
    public float jumpForce = 2f;
    public float runDuration = 2f;
    public float recoveryTime = 2f;
    public float slideFriction = 5f;
    public float slideStopThreshold = 0.2f;

    [Header("Configuración de Colisión")]
    public float ceilDistance = 1f;
    public float RayDistance = 0.35f;
    public float PickupRange = 1f;

    #endregion

    #region Variables Privadas

    // Estados de Entrada
    private bool runKey;
    private bool jumpKey;
    private bool crouchKey;
    private bool shellKey;

    // Estados del Jugador
    private bool grounded;
    private bool isInWater = false;
    private bool ShellRemoved;
    private bool crouched;
    private bool isSliding = false;
    private bool isAscending;
    private bool isDescending;


    // Movimiento
    private Vector2 move;
    private Vector2 normal;
    private Vector3 m_Velocity = Vector3.zero;
    private float previousYPosition = 0f;

    // Temporizadores
    private float runTimer = 0f;
    private float recoveryTimer = 0f;
    private float initialSlideSpeed = 8f;
    private float currentSlideSpeed;

    #endregion

    #region Métodos Unity

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        INSTANCE = this;
    }

    // Update is called once per frame
    void Update()
    {
        move.x = Input.GetAxisRaw("Horizontal");
        move.y = Input.GetAxisRaw("Vertical");
        runKey = Input.GetKey(KeyCode.LeftShift);
        jumpKey = Input.GetKeyDown(KeyCode.W);
        crouchKey = Input.GetKey(KeyCode.S);
        shellKey = Input.GetKeyDown(KeyCode.Q);

        if (jumpKey && !isInWater && grounded && ShellController.ShellRemoved())
        {
            cRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        PlayerRun();
        PlayerOrientation();
        PlayerCrouched();
        HandleShell();
        PlayerSlide();
        CheckSlopeState();
    }

    void FixedUpdate()
    {
        Vector2 targetVelocity;

        if (isInWater)
        {
            targetVelocity = new Vector2(move.x * swimSpeed, move.y * swimSpeed);
        }
        else
        {
            Vector2 dir = new Vector2(normal.y, normal.x) * move.x;
            targetVelocity = new Vector2(move.x * Speed, cRigidbody.linearVelocity.y);
        }

        cRigidbody.linearVelocity = Vector3.SmoothDamp(cRigidbody.linearVelocity, targetVelocity, ref m_Velocity, 0.05f);

        PlayerGrounded();
        PlayerCrouched();
    }

    // Código entra en el agua
    private void OnTriggerEnter2D(Collider2D enter)
    {
        if (enter.gameObject.tag == "Water")
        {
            cRigidbody.gravityScale = 0f;
            isInWater = true;
        }
    }

    // Código sale del agua
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Water")
        {
            cRigidbody.gravityScale = 1f;
            isInWater = false;
        }
    }

    // Código para cambiar el render de izquierda a derecha
    void PlayerOrientation()
    {
        if (move.x < 0)
            cRenderer.flipX = true;
        else if (move.x > 0)
            cRenderer.flipX = false;
    }

    // Detectar que el jugador toca el suelo
    void PlayerGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.up * 0.2f, Vector2.down, RayDistance, LayerMask.GetMask("Environment"));
        RaycastHit2D surfaceHit = Physics2D.Raycast(transform.position + Vector3.up * 1f, Vector2.down, 10f, LayerMask.GetMask("Environment"));

        normal = surfaceHit ? hit.normal : Vector3.down;

        if (hit && !grounded)
        {
            grounded = true;
        }
        else if (!hit)
        {
            grounded = false;
        }
    }

    // Detectar al jugador agachado
    void PlayerCrouched()
    {
        if (grounded && !isInWater)
        {
            bool hit = Physics2D.Raycast(transform.position + Vector3.up * 0.1f, Vector2.up, ceilDistance, LayerMask.GetMask("Environment"));
            bool isCrouched = hit || crouchKey;

            if (isCrouched && ShellController.ShellOn())
            {
                crouched = true;
                cHealth.NoDamage = true;
                if (!isSliding)
                {
                    cRigidbody.linearVelocity = new Vector2(0f, cRigidbody.linearVelocity.y);
                }
            }
            else if (isCrouched && ShellController.ShellRemoved())
            {
                crouched = true;
                cCollider.size = new Vector2(cCollider.size.x, 0.17f);
                cCollider.offset = new Vector2(0, 0.09f);
            }
            else
            {
                cCollider.size = new Vector2(cCollider.size.x, 0.23f);
                cCollider.offset = new Vector2(0, 0.13f);
                crouched = false;
                cHealth.NoDamage = false;
            }
        }
    }

    // Salir del caparazón
    void HandleShell()
    {
        if (grounded && shellKey)
        {
            ShellRemoved = !ShellRemoved;

            if (ShellRemoved)
            {
                ShellController.ShellTaked();
            }
            else
            {
                float DistancetoShell = Vector3.Distance(transform.position, ShellController.transform.position);
                if (DistancetoShell <= PickupRange)
                {
                    ShellController.ShellPut(transform);
                }
                else
                {
                    ShellRemoved = true;
                }
            }
        }
    }

    // Correr
    private void PlayerRun()
    {
        if (runKey && recoveryTimer <= 0f && ShellController.ShellOn() && Mathf.Abs(move.x) > 0f)
        {
            if (runTimer > 0f)
            {
                Speed = 8f;
                runTimer -= Time.deltaTime;
            }
            else
            {
                recoveryTimer = recoveryTime;
            }
        }
        else if (runKey && ShellController.ShellRemoved())
        {
            Speed = 8f;
        }
        else
        {
            Speed = 5f;

            if (recoveryTimer > 0f)
            {
                recoveryTimer -= Time.deltaTime;
            }
            else if (!runKey && runTimer < runDuration)
            {
                runTimer += Time.deltaTime;
            }
        }
    }

    // Hace que el jugador se pueda deslizar
    private void PlayerSlide()
    {
        if (grounded && crouchKey && Speed == 8f && ShellController.ShellOn())
        {
            if (!isSliding)
            {
                isSliding = true;
                currentSlideSpeed = initialSlideSpeed + 1;
            }
        }
        if (isSliding)
        {
            if (!isDescending)
            {
                currentSlideSpeed -= Time.deltaTime * slideFriction;
                if (currentSlideSpeed < 0)
                {
                    currentSlideSpeed = 0f;
                }
            }
            else
            {
                currentSlideSpeed = initialSlideSpeed + 1;
            }
            float slideDirection = cRenderer.flipX ? -1f : 1f;
            cRigidbody.linearVelocity = new Vector2(currentSlideSpeed * slideDirection, cRigidbody.linearVelocity.y);

            if (currentSlideSpeed <= 0.3f)
            {
                isSliding = false;
                if (crouchKey)
                {
                    cRigidbody.linearVelocity = new Vector2(0f, cRigidbody.linearVelocity.y);
                }
            }
        }
    }
    
    //Comprobamos si el jugador esta descendiendo o ascendiendo
    void CheckSlopeState()
    {   if ((normal.x < 0 && move.x < 0) || (normal.x > 0 && move.x > 0) && normal.y < 1)
        {
            if (normal.y < 1)
            {
                isDescending = true;
                isAscending = false;
            }
            else
            {
                isDescending = false;
                isAscending = true;
            }
        }
        else
        {
            isDescending = false ;
            isAscending = false;
        }
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    // }

    #endregion
}

