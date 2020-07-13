using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using System;

public class PlayerControl : MonoBehaviour
{
    public Player player; //Rewired component
    public Fighter fighter;
    public BasicAttack attack;
    public float weight;
    public float attackPower = 1;
    public float defensePower = 1;
    public float HP;
    public float damage = 0;
    public bool isBlocking = false;
    private bool isPlayer; //if false, it is an AI player
    public Animator anim;
    protected bool leaping; //is it in mid-leap?
    [SerializeField] protected bool AimingRight = true;
    public int playerID;
    [SerializeField] protected bool faceRight = true;
    public float Horiz;
    public float Vert;
    public float moveSpeed = 1000;
    [SerializeField] protected float jumpHeight = 10;
    public float speed;
    public float leapSpeed;
    public bool canMove = true;
    public bool grounded;
    public bool action = false;
    public float actionCooldown = -0.5f;
    public bool jump;
    public bool canInput;
    public bool isMoving;
    [SerializeField] protected Rigidbody2D rigidBody;
    public int maxJumpCount;
    public int jumpCount =0;
    public Transform GroundCheck;
    public Transform LeapAim;
    public float checkRadius = 0.2f;
    public LayerMask floor;
    public LayerMask foe;
    public float aimSpeed;
    public LineRenderer lineRenderer;
    public float aimRange;
    public GravAttractor Target;
    [SerializeField] protected GravAttractor storedTarget;
    public Vector2 LeapTarget;
    public float leapCooldown = 3f;
    public float aim;
    public float aimExtend = 0.25f;
    public bool isDamaged = false;
    protected bool cancelled = false;
    public int attackingWith;
    public int currentCombo = 0;
    protected float lastComboTime = 0f;
    protected float maxComboDelay = 0.9f;
    public float localHoriz;
    public float localVert;
    public GameObject Shaker;

    private void Awake()
    {
        player = ReInput.players.GetPlayer(playerID); //The player controlling this fighter
    }

    void Start()
    {
        Shaker = GameObject.FindGameObjectWithTag("Shake");
        HP = fighter.HitPoints;
        weight = fighter.weight;
        jumpHeight = fighter.jumpHeight;
        moveSpeed = fighter.speed;
        canInput = true;
        rigidBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        Target = GameObject.FindGameObjectWithTag("Floor").GetComponent<GravAttractor>();

    }

    // Update is called once per frame
    public virtual void Update()
    {
        GetComponent<GravBody>().attractor = Target;
        grounded = Physics2D.OverlapCircle(GroundCheck.position, checkRadius, floor);
       
        if (grounded)
        {
            jumpCount = 0;
        }
        isPlayer = gameObject.tag == "Player";
        if (isPlayer)
        {
            GetInput();
            ProccessInput();
        }
        actionCooldown += Time.deltaTime;
        leapCooldown += Time.deltaTime;
        if (Time.time - lastComboTime >= maxComboDelay)
        {
            currentCombo = 0;
        }
    }

    public virtual void FixedUpdate()
    {
        aim = (Mathf.PingPong(aimSpeed * Time.fixedTime, aimRange));
        anim.SetBool("damaged", isDamaged);
        anim.SetBool("grounded", grounded);
        anim.SetBool("leap", lineRenderer.enabled);
        anim.SetFloat("speed", speed);

        
        if (canMove)
        {
            switch (GetComponent<GravBody>().attractor.gameObject.tag)
            {
                case "Floor":
                    if ((faceRight && Horiz < 0) || (!faceRight && Horiz > 0))
                    {
                        Flip();
                    }
                    localHoriz = Math.Abs(Horiz);
                    localVert = Vert;
                    break;
                case "Cieling":
                    if ((faceRight && Horiz > 0) || (!faceRight && Horiz < 0))
                    {
                        Flip();
                    }
                    localHoriz = Math.Abs(Horiz);
                    localVert = -Vert;
                    break;
                case "Left":
                    if ((faceRight && Vert > 0) || (!faceRight && Vert < 0))
                    {
                        Flip();
                    }
                    localHoriz = Math.Abs(Vert);
                    localVert = Horiz;
                    break;
                case "Right":
                    if ((faceRight && Vert < 0) || (!faceRight && Vert > 0))
                    {
                        Flip();
                    }
                    localHoriz = Math.Abs(Vert);
                    localVert = -Horiz;
                    break;
                default:
                    if ((faceRight && Horiz < 0) || (!faceRight && Horiz > 0))
                    {
                        Flip();
                    }
                    localHoriz = Horiz;
                    localVert = Vert;
                    break;
            }
        }
    }

    void GetInput()
    {
        Horiz = player.GetAxis("Horizontal");
        Vert = player.GetAxis("Vertical");
        if (jumpCount <= maxJumpCount)
        {
            jump = player.GetButtonDown("Jump");
        }
        else jump = false;


        Vector2 mover;
            switch (GetComponent<GravBody>().attractor.gameObject.tag)
            {
                case "Floor":
                if (canMove)
                {
                    mover = new Vector2(Horiz * moveSpeed, 0);
                }
                else
                {
                    mover = new Vector2((Horiz * moveSpeed) / 5, 0);
                }
                if ((Horiz > 0) && (!action))
                    {
                        isMoving = true;
                        speed = Mathf.Abs(Horiz);
                    }
                    if ((Horiz < 0) && (!action))
                    {
                        isMoving = true;
                        speed = Mathf.Abs(Horiz);
                    }
                    if (Horiz == 0)
                    {
                        isMoving = false;
                    speed = 0;
                    }
                    break;
                case "Cieling":
                if (canMove)
                {
                    mover = new Vector2(Horiz * moveSpeed, 0);
                }
                else
                {
                    mover = new Vector2((Horiz * moveSpeed) / 5, 0);
                }
                if ((Horiz < 0) && (!action))
                    {
                        speed = Mathf.Abs(Horiz);
                        isMoving = true;
                        speed = Mathf.Abs(Horiz);
                    }
                    if ((Horiz > 0) && (!action))
                    {
                        isMoving = true;
                        speed = Mathf.Abs(Horiz);
                    }
                    if (Horiz == 0)
                    {
                        isMoving = false;
                    speed = 0;
                }
                    break;
                case "Left":
                if (canMove)
                {
                    mover = new Vector2(0, Vert * moveSpeed);
                }
                else
                {
                    mover = new Vector2(0, (Vert * moveSpeed)/5);
                }
                if ((Vert < 0) && (!action))
                    {
                        speed = Mathf.Abs(Vert);
                        isMoving = true;
                        speed = Mathf.Abs(Vert);
                    }
                    if ((Vert > 0) && (!action))
                    {
                        isMoving = true;
                        speed = Mathf.Abs(Vert);
                    }
                    if (Vert == 0)
                    {
                        isMoving = false;
                    speed = 0;
                }
                    break;
                case "Right":
                if (canMove)
                {
                    mover = new Vector2(0, Vert * moveSpeed);
                }
                else
                {
                    mover = new Vector2(0, (Vert * moveSpeed) / 5);
                }
                if ((Vert > 0) && (!action))
                    {
                        speed = Mathf.Abs(Vert);
                        isMoving = true;
                        speed = Mathf.Abs(Vert);
                    }
                    if ((Vert < 0) && (!action))
                    {
                        isMoving = true;
                        speed = Mathf.Abs(Vert);
                    }
                    if (Vert == 0)
                    {
                        isMoving = false;
                    speed = 0;
                }
                    break;
                default:
                    mover = new Vector2(Horiz * moveSpeed, 0);
                    if ((Horiz > 0) && (!action))
                    {
                        speed = Mathf.Abs(Horiz);
                        faceRight = true;
                        isMoving = true;
                        speed = Mathf.Abs(Horiz);
                    }
                    if ((Horiz < 0) && (!action))
                    {
                        faceRight = false;
                        isMoving = true;
                        speed = Mathf.Abs(Horiz);
                    }
                    if (Horiz == 0)
                    {
                        isMoving = false;
                    speed = 0;
                }
                    break;
            }

        if (grounded == true)
        {
            leaping = false;
            if (actionCooldown > 0.5f)
            {
                canInput = true;
                if ((canMove) && (!action))
                {
                    rigidBody.AddForce(mover);
                }



                if ((!cancelled) && leapCooldown >= 3f)
                {
                    if (player.GetButton("Leap"))
                    {

                        action = true;
                        LeapTarget = AimLeap(1);
                        canMove = false;
                        if (player.GetButtonDown("Attack") || (player.GetButtonDown("Special")))
                        {
                            cancelled = true;
                            storedTarget = Target;
                            lineRenderer.enabled = false;
                            canMove = true;
                            action = false;
                        }
                        if ((player.GetButtonDown("Attack") || (player.GetButtonDown("Special"))))
                        {
                            lineRenderer.enabled = false;
                            storedTarget = Target;
                            cancelled = true;
                            canMove = true;

                        }
                    }

                    if (player.GetNegativeButton("Leap"))
                    {
                        action = true;
                        LeapTarget = AimLeap(2);
                        canMove = false;

                        if ((player.GetButtonDown("Attack") || (player.GetButtonDown("Special"))))
                        {
                           
                            lineRenderer.enabled = false;
                            storedTarget = Target;
                            cancelled = true;
                            canMove = true;

                        }
                    }
                }


                if (player.GetButtonUp("Leap") || player.GetNegativeButtonUp("Leap"))
                {
                    if ((!cancelled) && leapCooldown >= 3)
                    {
                        Leap();
                    }
                    cancelled = false;
                    canMove = true;
                    action = false;
                }
            }
        }
            if (canMove) //if the fighter is able to move
            {
                if (actionCooldown >= 0)
                {
                if (player.GetButtonDown("Attack") && localHoriz <= 0.5 && localVert == 0)
                {
                    if (grounded)
                    {
                        if (currentCombo < 1)
                        {
                            attack = fighter.techniques[0];
                            anim.Play(attack.animationName);
                            lastComboTime = Time.time;
                            currentCombo++;
                            actionCooldown = -attack.recharge;
                        }
                        else
                        {
                            attack = fighter.techniques[1];
                            anim.Play(attack.animationName);
                            lastComboTime = Time.time;
                            currentCombo++;
                        }
                    }
                    if (!grounded)
                    {
                        attack = fighter.techniques[5];
                        anim.Play(attack.animationName);
                        lastComboTime = Time.time;
                        currentCombo++;
                        action = true;
                        actionCooldown = -attack.recharge;
                        canMove = false;
                    }
                }
                if (player.GetButtonDown("Attack") && localHoriz > 0.5f)
                {
                    if (grounded)
                    {
                        attack = fighter.techniques[2];
                        anim.Play(attack.animationName);
                        lastComboTime = Time.time;
                        currentCombo++;
                        action = true;
                        actionCooldown = -attack.recharge;
                        canMove = false;
                    }
                    if (!grounded)
                    {
                        attack = fighter.techniques[6];
                        anim.Play(attack.animationName);
                        lastComboTime = Time.time;
                        currentCombo++;
                        action = true;
                        actionCooldown = -attack.recharge;
                    }
                }

                if (player.GetButtonDown("Attack") && localVert < -0.75f)
                {
                    if (grounded)
                    {
                        attack = fighter.techniques[3];
                        anim.Play(attack.animationName);
                        lastComboTime = Time.time;
                        currentCombo++;
                        action = true;
                        actionCooldown = -attack.recharge;
                        canMove = false;
                    }
                }

                if (player.GetButtonDown("Attack") && localVert > 0.75f)
                {
                    if (grounded)
                    {
                        attack = fighter.techniques[4];
                        anim.Play(attack.animationName);
                        lastComboTime = Time.time;
                        currentCombo++;
                        action = true;
                        actionCooldown = -attack.recharge;
                        canMove = false;
                    }
                }
                }
                    
                    if (player.GetButtonDown("Special"))
                    {
                    }
                }
            
     

    }
    void ProccessInput()
    {
        if (jump)
        {
            Jump();
        }
    }
    void Flip()
    {
        faceRight = !faceRight;
        transform.Rotate(0, 180f, 0);
    }
    void Jump()
    {
        jumpCount += 1;
        switch (GetComponent<GravBody>().attractor.gameObject.tag) {
            case "Floor":
        rigidBody.AddForce(new Vector2(0, jumpHeight), ForceMode2D.Impulse);
                break;
            case "Cieling":
                rigidBody.AddForce(new Vector2(0, -jumpHeight), ForceMode2D.Impulse);
                break;
            case "Left":
                rigidBody.AddForce(new Vector2(jumpHeight, 0), ForceMode2D.Impulse);
                break;
            case "Right":
                rigidBody.AddForce(new Vector2(-jumpHeight, 0), ForceMode2D.Impulse);
                break;
            default:
                rigidBody.AddForce(new Vector2(0, jumpHeight), ForceMode2D.Impulse);
                break;
        }
    }

    Vector2 AimLeap(int pos)
    {
        if (!cancelled)
        {
            canMove = false;
            lineRenderer.enabled = true;
            RaycastHit2D hitInfo = Physics2D.Raycast(new Vector3(LeapAim.position.x, LeapAim.localPosition.y + aimExtend, LeapAim.position.z), LeapAim.up);
            if (hitInfo)
            {
                GameObject attck = hitInfo.transform.gameObject;
               
                GravAttractor wall = hitInfo.transform.GetComponent<GravAttractor>();
                GravBody foe = hitInfo.transform.GetComponent<GravBody>();
                if (wall != null)
                {
                    lineRenderer.SetPosition(0, LeapAim.position);
                    lineRenderer.SetPosition(1, hitInfo.point);
                    storedTarget = wall;
                }
                if ((foe != null) && (foe != this.GetComponent<GravBody>()))
                {
                    lineRenderer.SetPosition(0, LeapAim.position);
                    lineRenderer.SetPosition(1, hitInfo.point);
                    storedTarget = foe.GetComponent<GravBody>().attractor;
                }
                if (attck != null)
                {
                    PlayerControl Oattack = attck.GetComponent<PlayerControl>();
                    Projectile ammo = attck.GetComponent<Projectile>();
                    if ((Oattack != null && Oattack.attack != null) || ammo != null)
                    {
                        storedTarget = Target;
                        lineRenderer.SetPosition(0, LeapAim.position);
                        lineRenderer.SetPosition(1, hitInfo.point);
                    }
                    
                }
                if (wall == null && foe == null)
                {
                    storedTarget = Target;
                    lineRenderer.SetPosition(0, LeapAim.position);
                    lineRenderer.SetPosition(1, LeapAim.position + LeapAim.up * 1000);
                }
                LeapTarget = hitInfo.point;

                if (pos == 1)
                {
                    LeapAim.localEulerAngles = new Vector3(LeapAim.localRotation.x, LeapAim.localRotation.y, -aim);
                }
                if (pos == 2)
                    LeapAim.localEulerAngles = new Vector3(LeapAim.localRotation.x, LeapAim.localRotation.y, aim);
            }
            return LeapTarget;
        }
        else
        {
            storedTarget = Target;
            return transform.position;
        }
    }
    
    void Leap()
    {
       if(((Target.tag == "Floor") && (storedTarget.tag == "Cieling")) ||((Target.tag == "Cieling") && (storedTarget.tag == "Floor")) || ((Target.tag == "Left") && (storedTarget.tag == "Right")) || ((Target.tag == "Right") && (storedTarget.tag == "Left")))
        {
            faceRight = !faceRight;

        }

        if (LeapTarget != null && storedTarget != Target)
        {
            Target = storedTarget;
        
            //grounded = false;
            //leaping = true;
            transform.position = LeapTarget;
            if (Target.tag == "Floor" || Target.tag == "Cieling")
            {
                Shaker.GetComponent<shake>().Shaking("top");
            }
            if (Target.tag == "Right" || Target.tag == "Left")
            {
                Shaker.GetComponent<shake>().Shaking("side");
            }
        }

        if (LeapTarget != null && storedTarget == Target)
        {
            storedTarget = Target;
            transform.position = LeapTarget;

        }
        lineRenderer.enabled = false;
        leapCooldown = 0;
    }
   public void Land()
    {
        canInput = true;
        canMove = true;
        action = false;
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "HitBox")
        {
            if (!isBlocking)
            {
                if (collision.gameObject.GetComponent<AttackController>().priorityPower >= fighter.BaseDefense)
                {
                    Vector2 knock = new Vector2();
                    Vector2 source = new Vector2();
                    switch (GetComponent<GravBody>().attractor.gameObject.tag)
                    {
                        case "Floor":
                            knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                            source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                            break;
                        case "Cieling":
                            knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                            source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                            break;
                        case "Left":
                            knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                            source = new Vector2(1, -(collision.transform.position.y - transform.position.y));
                            break;
                        case "Right":
                            knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                            source = new Vector2(1, -(collision.transform.position.y - transform.position.y));
                            break;
                        default:
                            knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                            source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                            break;
                    }
                   
                    isDamaged = true;
                    rigidBody.AddForceAtPosition(knock * source, collision.transform.localPosition, ForceMode2D.Force);
                }
                damage += collision.gameObject.GetComponent<AttackController>().damagePower;

            }

        }
    }

    public void unAttack()
    {
        attack = null;
        canMove = true;
        action = false;
    }
}
