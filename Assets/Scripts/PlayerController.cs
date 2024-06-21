using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GG.Infrastructure.Utils.Swipe;
using UnityEngine.SceneManagement;
using TMPro;

namespace ChauDriver.Player {

[RequireComponent(typeof(CharacterController),typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float initialSpeed = 4.0f;
    [SerializeField] private float maximumSpeed = 30f;
    [SerializeField] private float increaseSpeedRate = 0.1f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float initialGravity = -9.81f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask turnLayer;
    [SerializeField] private AnimationClip slideAnimationClip;
    [SerializeField] private SwipeListener swipeListener;
    [SerializeField] private ParticleSystem particulas;
    [SerializeField] private LayerMask obstacleLayer;
    
    private int slidingAnimationId;
    private int slidingAnimationIdSliding;
    private bool sliding = false;
    private Animator animator;
    private Vector3 playerVelocity;
    private float gravity;
    private float playerSpeed;
    private Vector3 movementDirection = Vector3.forward;
    private CharacterController controller;
    public AudioSource src;
    public AudioClip sfx1,sfx2;
    private bool paqueterecogido;
    public TextMeshProUGUI dinero;
    public float dineroActual;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        slidingAnimationId = Animator.StringToHash("Running Jump");
        slidingAnimationIdSliding = Animator.StringToHash("Running Slide");
    }
    
    private void Start()
    {
        playerSpeed = initialSpeed;
        // Note: The line below doesn't seem necessary; it assigns the same value to itself.
        gravity = initialGravity;
        dineroActual = 0;
    }

    private void OnEnable() {
        swipeListener.OnSwipe.AddListener (OnSwipe);
    }

        private void OnSwipe(string swipe){
        switch (swipe){
            case "Left":
                PlayerTurn(swipe);
                Debug.Log("Left");
                break;
            case "Right":
                PlayerTurn(swipe);
                Debug.Log("Right");
                break;
            case "Up":
                PlayerJump();
                animator.Play(slidingAnimationId);
                src.clip = sfx1;
                src.Play();
                particulas.Play();                              
                Debug.Log("Up");
                break;
            case "Down":
                PlayerSlide();
                src.clip = sfx2;
                src.Play();
                particulas.Play();
                //animator.Play(slidingAnimationIdSliding);
                Debug.Log("Slide");
                break;
        }
    }

    private void OnDisable() {
        swipeListener.OnSwipe.RemoveListener(OnSwipe);
    }

    private void Update()
    {
        // Move the character forward based on playerSpeed.
        controller.Move(transform.forward * playerSpeed * Time.deltaTime);


        if(IsGrounded() && playerVelocity.y < 0){
            playerVelocity.y = 0f;
        }

        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void PlayerTurn(string turn){
        float turnValue = 0 ;
        if (turn == "Right"){
            turnValue = 1;
        }
        else if (turn == "Left"){
            turnValue = -1;
        }
        Vector3? turnPosition = checkTurn(turn); 
        Debug.Log($"Turn {turnPosition}");
        if (!turnPosition.HasValue){
            return;
        }
        Vector3 targetDirection = Quaternion.AngleAxis(90 * turnValue, Vector3.up) * movementDirection;
        Turn(turnValue,turnPosition.Value);

    }

    private void Turn(float turnValue,Vector3 turnPosition){
        Vector3 tempPlayerPosition = new Vector3(turnPosition.x,transform.position.y,turnPosition.z);
        controller.enabled = false;
        transform.position = tempPlayerPosition;
        controller.enabled = true;

        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0,90*turnValue,0);
        transform.rotation = targetRotation;
        movementDirection = transform.forward.normalized;
    }
    private Vector3? checkTurn(string turn){
        float turnValue = 0;
        if (turn == "Right"){
            turnValue = 1;
        }
        else if (turn == "Left"){
            turnValue = -1;
        }
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, .1f, turnLayer);
        if(hitColliders.Length != 0){
                Debug.Log("Entro en el tile");
            Tile tile = hitColliders[0].transform.parent.GetComponent<Tile>();
            TileType type = tile.type;
            Debug.Log(type);
            if ((type == TileType.LEFT ) || (type == TileType.RIGHT) || (type == TileType.SIDEWAYS) ){
                    Debug.Log(tile.pivot.position);
                return tile.pivot.position;
            }
        }
            Debug.Log("No entra");
        return null;
    }
    private void PlayerJump(){

        if (IsGrounded()) {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * gravity * -3f);
            controller.Move(playerVelocity * Time.deltaTime);
            
        }

    }
    private void PlayerSlide(){
        if(!sliding && IsGrounded()){
            StartCoroutine(Slide());
        }
    }
    private IEnumerator Slide(){
            sliding = true;

            // Ajustar el box collider para cuando está deslizándose
            Vector3 originalControllerCenter = controller.center;
            float originalControllerHeight = controller.height;

            // Ajustar la altura a la mitad y desplazar el centro hacia abajo
            controller.height = originalControllerHeight / 2;
            controller.center = new Vector3(originalControllerCenter.x,  -originalControllerCenter.y / 2, originalControllerCenter.z);

            // Iniciar la animación de deslizamiento
            animator.Play(slidingAnimationIdSliding);
            yield return new WaitForSeconds(slideAnimationClip.length);

            // Volver a colocar el box collider a la normalidad cuando acabe de deslizarse
            controller.height = originalControllerHeight;
            controller.center = originalControllerCenter;

            sliding = false;
        }
    private bool IsGrounded(float lenght = .2f) {
        Vector3 raycastOriginFirst = transform.position;
        raycastOriginFirst.y -= controller.height/2f;
        raycastOriginFirst.y += .1f;

        Vector3 raycastOriginSecond = raycastOriginFirst;
        raycastOriginFirst -= transform.forward * .2f;
        raycastOriginSecond += transform.forward * .2f;

        Debug.DrawLine(raycastOriginFirst,Vector3.down,Color.green,2f);
        Debug.DrawLine(raycastOriginFirst,Vector3.down,Color.red,2f);

        if(Physics.Raycast(raycastOriginFirst,Vector3.down,out RaycastHit hit, lenght, groundLayer) || Physics.Raycast(raycastOriginFirst,Vector3.down,out RaycastHit hit2, lenght, groundLayer)){
            return true;
        }
        return false;
    }


        private void GameOver()
        {
            SceneManager.LoadSceneAsync(2);
        }
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if(((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
            {
                GameOver();
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.CompareTag("Packet"))
            {
                collider.gameObject.SetActive(false);
                paqueterecogido = true;
            }
            if(collider.gameObject.CompareTag("Receive"))
            {
                dineroActual += 100;
                paqueterecogido = false;
                collider.gameObject.SetActive(false);
                UpdateMoney();
            }
        }

        void UpdateMoney()
        {
            dinero.text = " $ " + dineroActual;
        }
    }

}