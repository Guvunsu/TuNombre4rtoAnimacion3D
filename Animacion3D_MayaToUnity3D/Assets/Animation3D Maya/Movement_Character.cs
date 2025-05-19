using System.Collections;
using UnityEngine;

public class Movement_Character : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float playerSpeed = 1.0f;
    public float jumpHeight = 0.3f;
    private float gravityValue = -9.81f;
    private float rotationSpeed = 5f;
    private bool SitB = false;
    private bool MoveFree = true; //bloquea los controles de movimiento  
    private float interpolacion = 5f; //cambio sutil para el flotante del blendtree
    public float JumpOffset = 0.3f;
    public float UpOffset = 0.8f;
    public float WalkVelocity = 1f;
    public float RunVelocity = 2f;

    Animator anim;

    private void Start()
    {
        controller = gameObject.AddComponent<CharacterController>();
        anim = gameObject.GetComponent<Animator>();
        controller.center = new Vector3(0.0f, 1.0f, 0.0f); //estos parametros son según el personaje
        controller.height = 1.9f;
        controller.minMoveDistance = 0.00005f;
        controller.radius = 0.3f;
    }


    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        //movimiento
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        Vector3 move = (cameraForward.normalized * Input.GetAxis("Vertical")) + (cameraRight.normalized * Input.GetAxis("Horizontal"));

        // Calcula la velocidad actual del personaje
        float currentSpeed = move.magnitude;

        if (MoveFree)
        {
            controller.Move(move * Time.deltaTime * playerSpeed);

            if (move != Vector3.zero && groundedPlayer)
            {
                float movimientoHorizontal = Input.GetAxis("Horizontal");
                float movimientoVertical = Input.GetAxis("Vertical");

                // magnitud del vector
                Vector2 movimiento = new Vector2(movimientoHorizontal, movimientoVertical);
                float magnitudMovimiento = movimiento.magnitude;

                if (magnitudMovimiento > 1f)
                {
                    movimiento.Normalize();
                    magnitudMovimiento = 1f;
                }
                float multiplicadorVelocidad = Input.GetKey(KeyCode.LeftShift) ? 1f : 0.5f;
                // interpolación gradual
                float objetivoMagnitud = magnitudMovimiento * multiplicadorVelocidad;
                float magnitudMovimientoSuavizada = Mathf.Lerp(anim.GetFloat("Vertical"), objetivoMagnitud, interpolacion * Time.deltaTime);
                // Guardar en variable float del animator
                anim.SetFloat("Vertical", magnitudMovimientoSuavizada);

            }

            // Si el personaje no se está moviendo, gradualmente disminuye "Vertical" hacia 0
            if (currentSpeed < 0.1f)
            {
                float smoothVertical = Mathf.Lerp(anim.GetFloat("Vertical"), 0f, Time.deltaTime * interpolacion);
                anim.SetFloat("Vertical", smoothVertical);
            }

            if (move != Vector3.zero)
            {
                // mirar a rotación
                Quaternion newRotation = Quaternion.LookRotation(move);

                // Rotación
                gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, newRotation, Time.deltaTime * rotationSpeed);
            }

            //correr  
            if (Input.GetKey(KeyCode.LeftShift) && move != Vector3.zero && groundedPlayer)
            {
                playerSpeed = RunVelocity;

            }
            if (Input.GetKey(KeyCode.LeftShift) == false || move == Vector3.zero)
            {
                playerSpeed = WalkVelocity;
            }

            // Salto
            if (Input.GetButtonDown("Jump") && groundedPlayer)
            {
                StartCoroutine(JumpWait());
                anim.SetBool("Jump", true);
            }
            else
            {
                anim.SetBool("Jump", false);
            }

            //golpe
            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) && groundedPlayer)
            {
                StartCoroutine(ClickHitTime());
                anim.SetBool("Hit", true);
                if (groundedPlayer)
                    MoveFree = false;
            }
        }
        // Sentado
        if (Input.GetKeyDown(KeyCode.LeftControl) && groundedPlayer && !SitB)
        {
            anim.SetBool("Sit", true);
            MoveFree = false;
            SitB = true;
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl) && groundedPlayer && SitB)
        {
            anim.SetBool("Sit", false);

            SitB = false;
            StartCoroutine(UpWait());
        }
    }

    IEnumerator JumpWait()
    {
        yield return new WaitForSeconds(JumpOffset);
        playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
    }
    IEnumerator UpWait()
    {
        yield return new WaitForSeconds(UpOffset);
        MoveFree = true;

    }
    IEnumerator ClickHitTime()
    {
        yield return new WaitForSeconds(1.5f);
        anim.SetBool("Hit", false);
        MoveFree = true;

    }
}
