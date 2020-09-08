using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using System;

public class PlayerControl : MonoBehaviour
{
[Header("Components")]
    public Player player; //Rewired component
    public Fighter fighter;
    public Animator anim;
    public int playerID;
    [SerializeField] protected Rigidbody2D rigidBody;
    public Transform GroundCheck;
    public Transform LeapAim;
    public float checkRadius = 0.2f;
    public LayerMask floor;
    public LayerMask foe;
    public LineRenderer lineRenderer;
    public float aimRange;
    public GravAttractor Target;
    public string sTarget;
    [SerializeField] protected GravAttractor storedTarget;
    public Vector2 LeapTarget;
    public float aim;
    public float aimExtend = 0.25f;
    public GameObject Shaker;
    public float lowJumpMultiplier = 2f;
    public GameObject storedProjectile;
    public GameObject Shield;

    [Header("Controls")]
    public BasicAttack attack;
    public bool isBlocking = false;
    public bool isPlayer; //if false, it is an AI player
    protected bool leaping; //is it in mid-leap?
    [SerializeField] protected bool AimingRight = true;
    public bool faceRight = true;
    public float Horiz;
    public float Vert;
    public bool canMove = true;
    public bool grounded;
    public bool action = false;
    public float actionCooldown = -0.5f;
    public bool jump;
    public float jumpDelay = 0.25f;
    float jumpTimer;
    public bool canInput;
    public bool isMoving;
    public int jumpCount = 0;
    public float leapCooldown = 3f;
    public bool isDamaged = false;
    protected bool cancelled = false;
    public bool dodging = false;
    public int attackingWith;
    public int currentCombo = 0;
    public int attackCharge = 0;
    protected float lastComboTime = 0f;
    protected float maxComboDelay = 0.9f;
    public float localHoriz;
    public float localVert;

    [Header("Stats")]
    public float HP;
    public float attackPower = 1;
    public float defensePower = 1;
    public float Guard = 100;
    public float damage = 0;
    public float moveSpeed = 1000;
    [SerializeField] protected float jumpHeight = 10;
    public float speed;
    public float leapSpeed;
    public int maxJumpCount;
    public float aimSpeed;
    public float maxGrip;
    public float gripLoss;
    public bool vulnerable;


    private void Awake()
    {
        player = ReInput.players.GetPlayer(playerID); //The player controlling this fighter
        gameObject.name = fighter.name.ToString() + playerID;
    }

    void Start()
    {
        Shaker = GameObject.FindGameObjectWithTag("Shake");
        maxGrip = fighter.weight;
        HP = fighter.HitPoints;
        jumpHeight = fighter.jumpHeight;
        moveSpeed = fighter.speed;
        canInput = true;
        rigidBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        Target = GameObject.FindGameObjectWithTag("Floor").GetComponent<GravAttractor>();
        Shield = GetComponentInChildren<Barrier>(true).gameObject;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        GetComponent<GravBody>().attractor = Target;
        grounded = Physics2D.OverlapCircle(GroundCheck.position, checkRadius, floor);
       

        if (grounded)
        {
            jumpCount = 0;
            if (!isDamaged)
            {
                gripLoss -= 0.05f * Time.deltaTime;
                if (gripLoss < 1)
                {
                    gripLoss = 1;
                }
                if (gripLoss > maxGrip)
                {
                    gripLoss = maxGrip;
                }
            }
        }
        if ((dodging) && !grounded)
        {
                GetComponent<GravBody>().gravity = 0.25f;
            if ((Horiz <= 0.75) && (Vert <= 0.75) && (Horiz >= -0.75) && (Vert >= 0.75))
            {
                rigidBody.velocity = Vector2.zero * Time.deltaTime;
            }
            if (Horiz > 0.75)
            {
                rigidBody.velocity = Vector2.right * Time.deltaTime;
            }
            if (Horiz < -0.75)
            {
                rigidBody.velocity = Vector2.left * Time.deltaTime;
            }
            if (Vert > 0.75)
            {
                rigidBody.velocity = Vector2.up * Time.deltaTime;
            }
            if (Vert < -0.75)
            {
                rigidBody.velocity = Vector2.down * Time.deltaTime;
            }
        }
        if (!dodging)
        {
            GetComponent<GravBody>().gravity = -10;
            switch (GetComponent<GravBody>().attractor.gameObject.tag)
            {
                case "Floor":
                    if (rigidBody.velocity.y < 0)
                    {
                        rigidBody.velocity += Vector2.up * GetComponent<GravBody>().gravity * Time.deltaTime;
                    }
                    else
                    if (rigidBody.velocity.y > 0 && !player.GetButton("Jump"))
                    {
                        rigidBody.velocity += Vector2.up * GetComponent<GravBody>().gravity * (lowJumpMultiplier - 1) * Time.deltaTime;
                    }
                    break;
                case "Cieling":
                    if (rigidBody.velocity.y > 0)
                    {
                        rigidBody.velocity += Vector2.down * GetComponent<GravBody>().gravity * Time.deltaTime;
                    }
                    else
                    if (rigidBody.velocity.y < 0 && !player.GetButton("Jump"))
                    {
                        rigidBody.velocity += Vector2.down * GetComponent<GravBody>().gravity * (lowJumpMultiplier - 1) * Time.deltaTime;
                    }
                    break;
                case "Left":
                    if (rigidBody.velocity.x < 0)
                    {
                        rigidBody.velocity += Vector2.right * GetComponent<GravBody>().gravity * Time.deltaTime;
                    }
                    else
                    if (rigidBody.velocity.x > 0 && !player.GetButton("Jump"))
                    {
                        rigidBody.velocity += Vector2.right * GetComponent<GravBody>().gravity * (lowJumpMultiplier - 1) * Time.deltaTime;
                    }
                    break;
                case "Right":
                    if (rigidBody.velocity.x > 0)
                    {
                        rigidBody.velocity += Vector2.left * GetComponent<GravBody>().gravity * Time.deltaTime;
                    }
                    else
                    if (rigidBody.velocity.x < 0 && !player.GetButton("Jump"))
                    {
                        rigidBody.velocity += Vector2.left * GetComponent<GravBody>().gravity * (lowJumpMultiplier - 1) * Time.deltaTime;
                    }
                    break;
                default:

                    break;
            }
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
        sTarget = Target.gameObject.tag;
    }

    public virtual void FixedUpdate()
    {
        if (player.GetButton("Leap") || player.GetNegativeButton("Leap"))
        {
            aim = (Mathf.PingPong(aimSpeed * Time.fixedTime, aimRange));
        }
        else
        {
            aim = 0;
        }
        if (damage > HP)
        {
            anim.SetTrigger("Dead");
            GetComponentInParent<PlayerTracker>().defeated = true;
        }
        anim.SetBool("damaged", isDamaged);
        anim.SetBool("grounded", grounded);
        anim.SetBool("leap", lineRenderer.enabled);
        anim.SetFloat("speed", speed);
        anim.SetInteger("charge", attackCharge);
        if (grounded)
        {
            anim.SetBool("Guard", isBlocking);
        }
        if (!grounded)
        {
            if ((player.GetButtonDown("Block")) && actionCooldown > 0)
            {
                anim.SetTrigger("Dodge");
            }
        }
        if ((Guard < 100) && (!isBlocking))
        {
            Guard += 0.4f;
        }
        if (Guard >= 100)
        {
            Guard = 100;
        }
            if (Guard < 0)
        {
            Guard = 0;
        }
        if (isBlocking)
        {
            Guard -= 0.1f;
        }
        if (canMove && !isBlocking)
        {
            switch (GetComponent<GravBody>().attractor.gameObject.tag)
            {
                case "Floor":
                    if ((faceRight && Horiz < 0) || (!faceRight && Horiz > 0))
                    {
                        Flip();
                    }
                    localHoriz = Horiz;
                    localVert = Vert;
                    break;
                case "Cieling":
                    if ((faceRight && Horiz > 0) || (!faceRight && Horiz < 0))
                    {
                        Flip();
                    }
                    localHoriz = Horiz;
                    localVert = -Vert;
                    break;
                case "Left":
                    if ((faceRight && Vert > 0) || (!faceRight && Vert < 0))
                    {
                        Flip();
                    }
                    localHoriz = Vert;
                    localVert = Horiz;
                    break;
                case "Right":
                    if ((faceRight && Vert < 0) || (!faceRight && Vert > 0))
                    {
                        Flip();
                    }
                    localHoriz = Vert;
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
            if (jumpTimer > Time.time)
            {
                Jump();
            }
        }
    }

    void GetInput()
    {
        Horiz = player.GetAxis("Horizontal");
        Vert = player.GetAxis("Vertical");
        if ((jumpCount <= maxJumpCount) && !action)
        {
            jump = player.GetButtonDown("Jump");
        }
        if (grounded && !action && Guard > 0)
        {
            isBlocking = player.GetButton("Block");
            if (player.GetButtonUp("Block"))
            {
                Guarded();
            }
        }
        else jump = false;


        Vector2 mover;
            switch (GetComponent<GravBody>().attractor.gameObject.tag)
            {
                case "Floor":
                if (canMove)
                {
                    mover = new Vector2(Horiz * moveSpeed * Time.deltaTime, 0);
                }
                else
                {
                    mover = new Vector2((Horiz * moveSpeed * Time.deltaTime) / 5, 0);
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
                    mover = new Vector2(Horiz * moveSpeed * Time.deltaTime, 0);
                }
                else
                {
                    mover = new Vector2((Horiz * moveSpeed * Time.deltaTime) / 5, 0);
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
                    mover = new Vector2(0, Vert * moveSpeed * Time.deltaTime);
                }
                else
                {
                    mover = new Vector2(0, (Vert * moveSpeed * Time.deltaTime) /5);
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
                    mover = new Vector2(0, Vert * moveSpeed * Time.deltaTime);
                }
                else
                {
                    mover = new Vector2(0, (Vert * moveSpeed * Time.deltaTime) / 5);
                }
                if ((Vert > 0) && (!action))
                    {
                        speed = Mathf.Abs(Vert);
                        isMoving = true;
                        speed = Mathf.Abs(Vert);
                    }
                    if ((Vert < 0) && (!action))
                {
                    speed = Mathf.Abs(Vert);
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
                    mover = new Vector2(Horiz * moveSpeed * Time.deltaTime, 0);
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

        if (grounded)
        {
            leaping = false;
            if (actionCooldown > 0.5f)
            {
                canInput = true;
                if ((canMove) && (!action) && (!isBlocking)) 
                {
                    rigidBody.AddForce(mover * 100);
                    if (anim.GetBool("knocked out") == true)
                    {
                        anim.SetBool("knocked out", false);
                    }
                }

            }
        }
        if (!grounded)
        {
            if (actionCooldown > 0.5f)
            {
                canInput = true;
                if ((canMove) && (!action))
                {
                    rigidBody.AddForce(mover * 0.75f);
                }

            }
        }

        if ((!cancelled) && leapCooldown >= 3f)
                {
                    if (player.GetButton("Leap"))
                    {

                        action = true;
                        LeapTarget = AimLeap(1);
                        canMove = false;
                        if (isDamaged || player.GetButtonDown("Attack") || (player.GetButtonDown("Special")))
                        {
                            cancelled = true;
                            storedTarget = Target;
                            lineRenderer.enabled = false;
                            canMove = true;
                            action = false;
                        }
                        if ((isDamaged || player.GetButtonDown("Attack") || player.GetButtonDown("Special")))
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

                        if ((isDamaged ||player.GetButtonDown("Attack") || player.GetButtonDown("Special")))
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



        if (!isBlocking && !dodging)
        {
            if (canMove) //if the fighter is able to move
            {
                if (actionCooldown >= 0)
                {
                    if (player.GetButtonDown("Attack") && (localHoriz <= 0.5 && localHoriz >= -0.5) && localVert == 0)
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
                    if (player.GetButtonDown("Attack") && localHoriz > 0.5)
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
                            if (faceRight)
                            {
                                attack = fighter.techniques[6];
                                anim.Play(attack.animationName);
                                lastComboTime = Time.time;
                                currentCombo++;
                                action = true;
                                actionCooldown = -attack.recharge;
                            }
                            if (!faceRight)
                            {
                                attack = fighter.techniques[9];
                                anim.Play(attack.animationName);
                                lastComboTime = Time.time;
                                currentCombo++;
                                action = true;
                                actionCooldown = -attack.recharge;
                            }
                        }
                    }
                    if (player.GetButtonDown("Attack") && localHoriz < -0.5)
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
                            if (!faceRight)
                            {
                                attack = fighter.techniques[6];
                                anim.Play(attack.animationName);
                                lastComboTime = Time.time;
                                currentCombo++;
                                action = true;
                                actionCooldown = -attack.recharge;
                            }
                            if (faceRight)
                            {
                                attack = fighter.techniques[9];
                                anim.Play(attack.animationName);
                                lastComboTime = Time.time;
                                currentCombo++;
                                action = true;
                                actionCooldown = -attack.recharge;
                            }
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
                        if (!grounded)
                        {
                            attack = fighter.techniques[7];
                            anim.Play(attack.animationName);
                            lastComboTime = Time.time;
                            currentCombo++;
                            action = true;
                            actionCooldown = -attack.recharge;
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
                        if (!grounded)
                        {
                            attack = fighter.techniques[8];
                            anim.Play(attack.animationName);
                            lastComboTime = Time.time;
                            currentCombo++;
                            action = true;
                            actionCooldown = -attack.recharge;
                        }
                    }


                    if (player.GetButtonDown("Special") && (localHoriz < 0.75 && localHoriz > -0.75) && (localVert < 0.75 && localVert > -0.75))
                    {
                        if (grounded)
                        {
                            attack = fighter.techniques[10];
                            anim.Play(attack.animationName);
                            action = true;
                            lastComboTime = Time.time;
                            canMove = false;
                            if (attack.projectile != null)
                            {
                                storedProjectile = attack.projectile;
                            }
                            else return;

                            if (!attack.chargable)
                            {
                                if (storedProjectile != null)
                                {
                                    switch (Target.gameObject.tag)
                                    {
                                        case "Floor":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x + attack.projectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + attack.projectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x - attack.projectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + attack.projectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }

                                            }
                                            break;
                                        case "Cieling":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x - attack.projectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - attack.projectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x + attack.projectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - attack.projectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }

                                            }
                                            break;
                                        case "Right":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x + attack.projectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + attack.projectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x - attack.projectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + attack.projectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                            }
                                            break;
                                        case "Left":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x - attack.projectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y - attack.projectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x + attack.projectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + attack.projectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                            }
                                            break;
                                    }
                                    storedProjectile = null;
                                }
                                else
                                {
                                    currentCombo++;
                                    actionCooldown = -attack.recharge;
                                }

                            }
                            else
                            {
                                if (isDamaged)
                                {
                                    unAttack();
                                }

                            }
                            currentCombo++;
                            actionCooldown = -attack.recharge;
                        }

                        if (!grounded)
                        {
                            attack = fighter.techniques[11];
                            anim.Play(attack.animationName);
                            action = true;
                            lastComboTime = Time.time;
                            canMove = false;
                            if (attack.projectile != null)
                            {
                                storedProjectile = attack.projectile;
                            }
                            else return;

                            if (!attack.chargable)
                            {
                                if (storedProjectile != null)
                                {

                                    if (faceRight)
                                    {
                                        switch (Target.gameObject.tag)
                                        {
                                            case "Floor":
                                                Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x + attack.projectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + attack.projectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Cieling":
                                                Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x - attack.projectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - attack.projectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Right":
                                                Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x + attack.projectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + attack.projectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Left":
                                                Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x - attack.projectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y - attack.projectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                        }
                                    }
                                    if (!faceRight)
                                    {
                                        switch (Target.gameObject.tag)
                                        {
                                            case "Floor":
                                                Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x - attack.projectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + attack.projectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Cieling":
                                                Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x + attack.projectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - attack.projectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Right":
                                                Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x + attack.projectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y - attack.projectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Left":
                                                Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x - attack.projectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + attack.projectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    currentCombo++;
                                   
                                    actionCooldown = -attack.recharge;
                                }

                            }
                            else
                            {
                                if (isDamaged)
                                {
                                    unAttack();
                                }


                            }

                        }
                    }
                    if (player.GetButtonDown("Special") && (localHoriz >= 0.75 || localHoriz <= -0.75))
                    {
                        if (grounded)
                        {
                            attack = fighter.techniques[12];
                            anim.Play(attack.animationName);
                            action = true;
                            lastComboTime = Time.time;
                            canMove = false;
                            if (attack.projectile != null)
                            {
                                storedProjectile = attack.projectile;
                            }
                            else return;
                            if (!attack.chargable && !attack.delay)
                            {
                                if (storedProjectile != null)
                                {
                                    switch (Target.gameObject.tag)
                                    {
                                        case "Floor":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }

                                            }
                                            break;
                                        case "Cieling":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }

                                            }
                                            break;
                                        case "Right":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                            }
                                            break;
                                        case "Left":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    currentCombo++;
                                    actionCooldown = -attack.recharge;
                                }

                            }
                            else
                            {
                                if (isDamaged)
                                {
                                    unAttack();
                                }

                            }
                        }

                        if (!grounded)
                        {
                            attack = fighter.techniques[13];
                            anim.Play(attack.animationName);
                            action = true;
                            lastComboTime = Time.time;
                            canMove = false;
                            if (attack.projectile != null)
                            {
                                storedProjectile = attack.projectile;
                            }
                            else return;

                            if (!attack.chargable && !attack.delay)
                            {
                                if (storedProjectile != null)
                                {

                                    if (faceRight)
                                    {
                                        switch (Target.gameObject.tag)
                                        {
                                            case "Floor":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Cieling":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Right":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Left":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                        }
                                    }
                                    if (!faceRight)
                                    {
                                        switch (Target.gameObject.tag)
                                        {
                                            case "Floor":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Cieling":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Right":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Left":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    currentCombo++;
                                    actionCooldown = -attack.recharge;
                                }
                            }
                            else
                            {
                                if (isDamaged)
                                {
                                    unAttack();
                                }
                            }
                        }
                    }

                    if (player.GetButtonDown("Special") && (localVert <= -0.75))
                    {
                        if (grounded)
                        {
                            attack = fighter.techniques[14];
                            anim.Play(attack.animationName);
                            action = true;
                            lastComboTime = Time.time;
                            canMove = false;
                            if (attack.projectile != null)
                            {
                                storedProjectile = attack.projectile;
                            }
                            else return;
                            if (!attack.chargable && !attack.delay)
                            {
                                if (storedProjectile != null)
                                {
                                    switch (Target.gameObject.tag)
                                    {
                                        case "Floor":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }

                                            }
                                            break;
                                        case "Cieling":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }

                                            }
                                            break;
                                        case "Right":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                            }
                                            break;
                                        case "Left":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    currentCombo++;
                                    actionCooldown = -attack.recharge;
                                }

                            }
                            else
                            {
                                if (isDamaged)
                                {
                                    unAttack();
                                }

                            }
                        }

                        if (!grounded)
                        {
                            attack = fighter.techniques[15];
                            anim.Play(attack.animationName);
                            action = true;
                            lastComboTime = Time.time;
                            canMove = false;
                            if (attack.projectile != null)
                            {
                                storedProjectile = attack.projectile;
                            }
                            else return;

                            if (!attack.chargable && !attack.delay)
                            {
                                if (storedProjectile != null)
                                {

                                    if (faceRight)
                                    {
                                        switch (Target.gameObject.tag)
                                        {
                                            case "Floor":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Cieling":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Right":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Left":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                        }
                                    }
                                    if (!faceRight)
                                    {
                                        switch (Target.gameObject.tag)
                                        {
                                            case "Floor":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Cieling":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Right":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Left":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    currentCombo++;
                                    actionCooldown = -attack.recharge;
                                }
                            }
                            else
                            {
                                if (isDamaged)
                                {
                                    unAttack();
                                }
                            }
                        }
                    }
                    if (player.GetButtonDown("Special") && (localVert >= 0.75))
                    {
                        if (grounded)
                        {
                            attack = fighter.techniques[16];
                            anim.Play(attack.animationName);
                            action = true;
                            lastComboTime = Time.time;
                            canMove = false;
                            if (attack.projectile != null)
                            {
                                storedProjectile = attack.projectile;
                            }
                            else return;
                            if (!attack.chargable && !attack.delay)
                            {
                                if (storedProjectile != null)
                                {
                                    switch (Target.gameObject.tag)
                                    {
                                        case "Floor":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }

                                            }
                                            break;
                                        case "Cieling":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }

                                            }
                                            break;
                                        case "Right":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                            }
                                            break;
                                        case "Left":
                                            {
                                                if (faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                                if (!faceRight)
                                                {
                                                    Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                }
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    currentCombo++;
                                    actionCooldown = -attack.recharge;
                                }

                            }
                            else
                            {
                                if (isDamaged)
                                {
                                    unAttack();
                                }

                            }
                        }

                        if (!grounded)
                        {
                            attack = fighter.techniques[17];
                            anim.Play(attack.animationName);
                            action = true;
                            lastComboTime = Time.time;
                            canMove = false;
                            if (attack.projectile != null)
                            {
                                storedProjectile = attack.projectile;
                            }
                            else return;

                            if (!attack.chargable && !attack.delay)
                            {
                                if (storedProjectile != null)
                                {

                                    if (faceRight)
                                    {
                                        switch (Target.gameObject.tag)
                                        {
                                            case "Floor":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Cieling":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Right":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Left":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                        }
                                    }
                                    if (!faceRight)
                                    {
                                        switch (Target.gameObject.tag)
                                        {
                                            case "Floor":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Cieling":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Right":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x + storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y - storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                            case "Left":
                                                Instantiate<GameObject>(storedProjectile, new Vector2(transform.position.x - storedProjectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + storedProjectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    currentCombo++;
                                    actionCooldown = -attack.recharge;
                                }
                            }
                            else
                            {
                                if (isDamaged)
                                {
                                    unAttack();
                                }
                            }
                        }
                    }

                    if (player.GetButton("Attack") && player.GetButton("Special"))
                    {

                    }
                }
            }

            if (player.GetButtonUp("Special"))
            {
                if (storedProjectile != null)
                {
                    anim.SetTrigger("unleashed");
                    if (attackCharge >= 1)
                    {

                        if (faceRight)
                        {
                            switch (Target.gameObject.tag)
                            {
                                case "Floor":
                                    Instantiate<GameObject>(storedProjectile, new Vector2((transform.position.x + storedProjectile.GetComponent<Projectile>().ammo.launchPoint.x), (transform.position.y + storedProjectile.GetComponent<Projectile>().ammo.launchPoint.y)), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                    break;
                                case "Cieling":
                                    Instantiate<GameObject>(storedProjectile, new Vector2((transform.position.x - storedProjectile.GetComponent<Projectile>().ammo.launchPoint.x), (transform.position.y - storedProjectile.GetComponent<Projectile>().ammo.launchPoint.y)), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                    break;
                                case "Right":
                                    Instantiate<GameObject>(storedProjectile, new Vector2((transform.position.x + storedProjectile.GetComponent<Projectile>().ammo.launchPoint.y), (transform.position.y + storedProjectile.GetComponent<Projectile>().ammo.launchPoint.x)), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                    break;
                                case "Left":
                                    Instantiate<GameObject>(storedProjectile, new Vector2((transform.position.x - storedProjectile.GetComponent<Projectile>().ammo.launchPoint.y), (transform.position.y - storedProjectile.GetComponent<Projectile>().ammo.launchPoint.x)), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                    break;
                            }
                        }
                        if (!faceRight)
                        {
                            switch (Target.gameObject.tag)
                            {
                                case "Floor":
                                    Instantiate<GameObject>(storedProjectile, new Vector2((transform.position.x - storedProjectile.GetComponent<Projectile>().ammo.launchPoint.x), (transform.position.y + storedProjectile.GetComponent<Projectile>().ammo.launchPoint.y)), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                    break;
                                case "Cieling":
                                    Instantiate<GameObject>(storedProjectile, new Vector2((transform.position.x + storedProjectile.GetComponent<Projectile>().ammo.launchPoint.x), (transform.position.y - storedProjectile.GetComponent<Projectile>().ammo.launchPoint.y)), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                    break;
                                case "Right":
                                    Instantiate<GameObject>(storedProjectile, new Vector2((transform.position.x + storedProjectile.GetComponent<Projectile>().ammo.launchPoint.y), (transform.position.y - storedProjectile.GetComponent<Projectile>().ammo.launchPoint.x)), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                    break;
                                case "Left":
                                    Instantiate<GameObject>(storedProjectile, new Vector2((transform.position.x - storedProjectile.GetComponent<Projectile>().ammo.launchPoint.y), (transform.position.y + storedProjectile.GetComponent<Projectile>().ammo.launchPoint.x)), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                                    break;
                            }
                        }

                        currentCombo++;
                        actionCooldown = -attack.recharge;
                    }
                }
            }
        }
    }
    void ProccessInput()
    {
        if (jump)
        {
            jumpTimer = Time.time + jumpDelay;
        }
    }
    public void Flip()
    {
        if (grounded)
        {
            faceRight = !faceRight;
            GetComponentInParent<PlayerTracker>().FlipProjectiles();
            transform.Rotate(0, 180f, 0);
        }
    }

    void Jump()
    {
        jumpCount += 1;
        switch (GetComponent<GravBody>().attractor.gameObject.tag) {
            case "Floor":
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpHeight);
                break;
            case "Cieling":
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, -jumpHeight);
                break;
            case "Left":
                rigidBody.velocity = new Vector2(jumpHeight, rigidBody.velocity.y);
                break;
            case "Right":
                rigidBody.velocity = new Vector2(-jumpHeight, rigidBody.velocity.y);
                break;
            default:
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpHeight);
                break;
        }
        jumpTimer = 0;
    }

    Vector2 AimLeap(int pos)
    {
        if (LeapAim.localPosition.y != 0.25f)
        {
            LeapAim.localPosition = new Vector3(0, 0.25f, 0);
        }
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
        dodging = false;
        canInput = true;
        canMove = true;
        action = false;
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "HitBox")
        {
            
            if (!collision.GetComponent<AttackController>().isProjectile || (collision.GetComponent<AttackController>().isProjectile && (collision.gameObject.GetComponentInParent<PlayerTracker>().playerNumber != playerID)))
            {
                if ((!isBlocking) && (!dodging))
                {
                    Vector2 knock = new Vector2();
                    Vector2 source = new Vector2();
                    float back = (gripLoss / maxGrip) * 10;
                    if (collision.gameObject.GetComponent<AttackController>().priorityPower >= fighter.BaseDefense)
                    {
                        string sourceGrav = collision.gameObject.GetComponentInParent<PlayerTracker>().targetTag.gameObject.tag;
                        switch (GetComponent<GravBody>().attractor.gameObject.tag)
                        {
                            case "Floor":
                                if (sourceGrav == "Floor")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                    source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                }
                                if (sourceGrav == "Cieling")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                    source = new Vector2(-(collision.transform.position.x - transform.position.x), -1);
                                }
                                if (sourceGrav == "Left")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                    source = new Vector2(1, (collision.transform.position.x - transform.position.x));
                                }
                                if (sourceGrav == "Right")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                    source = new Vector2(-1, (collision.transform.position.x - transform.position.x));
                                }
                                break;
                            case "Cieling":
                                if (sourceGrav == "Floor")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                    source = new Vector2(-(collision.transform.position.x - transform.position.x), -1);
                                }
                                if (sourceGrav == "Cieling")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                    source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                }
                                if (sourceGrav == "Left")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                    source = new Vector2(1, (collision.transform.position.x - transform.position.x));
                                }
                                if (sourceGrav == "Right")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                    source = new Vector2(-1, (collision.transform.position.x - transform.position.x));
                                }
                                break;
                            case "Left":
                                if (sourceGrav == "Floor")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                    source = new Vector2((collision.transform.position.x - transform.position.x), 1);
                                }
                                if (sourceGrav == "Cieling")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                    source = new Vector2((collision.transform.position.x - transform.position.x), -1);
                                }
                                if (sourceGrav == "Left")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                    source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                }
                                if (sourceGrav == "Right")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                    source = new Vector2(-(collision.transform.position.x - transform.position.x), -1);
                                }
                                break;
                            case "Right":

                                if (sourceGrav == "Floor")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                    source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                }
                                if (sourceGrav == "Cieling")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                    source = new Vector2(-(collision.transform.position.x - transform.position.x), -1);
                                }
                                if (sourceGrav == "Left")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                    source = new Vector2(-(collision.transform.position.x - transform.position.x), -1);
                                }
                                if (sourceGrav == "Right")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                    source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                }
                                break;
                            default:
                                if (sourceGrav == "Floor")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                    source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                }
                                if (sourceGrav == "Cieling")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                    source = new Vector2(-(collision.transform.position.x - transform.position.x), -1);
                                }
                                if (sourceGrav == "Left")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                    source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                }
                                if (sourceGrav == "Right")
                                {
                                    knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                    source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                }
                                break;
                        }

                        isDamaged = true;
                        rigidBody.AddForceAtPosition(knock * source * back, collision.transform.localPosition, ForceMode2D.Force);
                    }
                    damage += collision.gameObject.GetComponent<AttackController>().damagePower;
                    gripLoss += collision.gameObject.GetComponent<AttackController>().gripLoss;
                    if (gripLoss >= maxGrip)
                    {
                       Launch(knock * source);
                    }
                }
                if (isBlocking)
                    {
                            Vector2 knock = new Vector2();
                            Vector2 source = new Vector2();
                            string sourceGrav = collision.gameObject.GetComponentInParent<PlayerTracker>().targetTag.gameObject.tag;
                            switch (GetComponent<GravBody>().attractor.gameObject.tag)
                            {
                                case "Floor":
                                    if (sourceGrav == "Floor")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                        source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                    }
                                    if (sourceGrav == "Cieling")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                        source = new Vector2(-(collision.transform.position.x - transform.position.x), -1);
                                    }
                                    if (sourceGrav == "Left")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                        source = new Vector2(1, (collision.transform.position.x - transform.position.x));
                                    }
                                    if (sourceGrav == "Right")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                        source = new Vector2(-1, (collision.transform.position.x - transform.position.x));
                                    }
                                    break;
                                case "Cieling":
                                    if (sourceGrav == "Floor")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                        source = new Vector2(-(collision.transform.position.x - transform.position.x), -1);
                                    }
                                    if (sourceGrav == "Cieling")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                        source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                    }
                                    if (sourceGrav == "Left")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                        source = new Vector2(1, (collision.transform.position.x - transform.position.x));
                                    }
                                    if (sourceGrav == "Right")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                        source = new Vector2(-1, (collision.transform.position.x - transform.position.x));
                                    }
                                    break;
                                case "Left":
                                    if (sourceGrav == "Floor")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                        source = new Vector2((collision.transform.position.x - transform.position.x), 1);
                                    }
                                    if (sourceGrav == "Cieling")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                        source = new Vector2((collision.transform.position.x - transform.position.x), -1);
                                    }
                                    if (sourceGrav == "Left")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                        source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                    }
                                    if (sourceGrav == "Right")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                        source = new Vector2(-(collision.transform.position.x - transform.position.x), -1);
                                    }
                                    break;
                                case "Right":

                                    if (sourceGrav == "Floor")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                        source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                    }
                                    if (sourceGrav == "Cieling")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                        source = new Vector2(-(collision.transform.position.x - transform.position.x), -1);
                                    }
                                    if (sourceGrav == "Left")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                        source = new Vector2(-(collision.transform.position.x - transform.position.x), -1);
                                    }
                                    if (sourceGrav == "Right")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                        source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                    }
                                    break;
                                default:
                                    if (sourceGrav == "Floor")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                        source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                    }
                                    if (sourceGrav == "Cieling")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.x * 3, collision.gameObject.GetComponent<AttackController>().knockback.y * 3);
                                        source = new Vector2(-(collision.transform.position.x - transform.position.x), -1);
                                    }
                                    if (sourceGrav == "Left")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                        source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                    }
                                    if (sourceGrav == "Right")
                                    {
                                        knock = new Vector2(collision.gameObject.GetComponent<AttackController>().knockback.y * 3, collision.gameObject.GetComponent<AttackController>().knockback.x * 3);
                                        source = new Vector2(-(collision.transform.position.x - transform.position.x), 1);
                                    }
                                    break;
                            }
                        if (GetComponent<GravBody>().attractor.gameObject.CompareTag("Floor") || GetComponent<GravBody>().attractor.gameObject.CompareTag("Cieling"))
                        {
                            rigidBody.AddForceAtPosition(new Vector2((knock.x * source.x) / 2, 0), collision.transform.localPosition, ForceMode2D.Force);
                        }
                        else
                        {
                            rigidBody.AddForceAtPosition(new Vector2(0, (knock.y * source.y) / 2), collision.transform.localPosition, ForceMode2D.Force);
                        }
                    Guard -= collision.gameObject.GetComponent<AttackController>().GuardBreaking;
                }
                       
                        }
            else return;
        }
    }

    
    public void unAttack()
    {
        attack = null;
        storedProjectile = null;
        canMove = true;
        action = false;
        attackCharge = 0;
    }
    public void Launch(Vector2 dir)
    {
        anim.SetBool("knocked out", true);
        canInput = false;
        canMove = false;
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, dir, 10000, floor);
        if (hitInfo)
        {
            GravAttractor wall = hitInfo.transform.GetComponent<GravAttractor>();
            storedTarget = wall; ;
            Target = storedTarget;
            Vector2 launchTarget = hitInfo.point;
            transform.position = launchTarget;
            if (Target.tag == "Floor" || Target.tag == "Cieling")
            {
                Shaker.GetComponent<shake>().Shaking("top");
            }
            if (Target.tag == "Right" || Target.tag == "Left")
            {
                Shaker.GetComponent<shake>().Shaking("side");
            }
            gripLoss = 1f;
        }
    }
    public void Guarded()
    {
        if (grounded)
        {
            if (isBlocking)
            {
                Shield.SetActive(true);
            }
            else
            {
                Shield.SetActive(false);
                actionCooldown = -0.4f;
            }
        }
        if (!grounded)
        {
            dodging = !dodging;
            actionCooldown = -0.6f;
            
        }
    }

    public void GuardBreak()
    {
        anim.SetTrigger("Broken");
        isBlocking = false;
        vulnerable = true;
    }
    public void ShakeOff()
    {
        vulnerable = false;
    }
    public void ChargeProjectile()
    {
        attackCharge++;
    }
    public void delayedAttack()
    {
        if (storedProjectile != null)
        {
            switch (Target.gameObject.tag)
            {
                case "Floor":
                    {
                        if (faceRight)
                        {
                            Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x + attack.projectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + attack.projectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }
                        if (!faceRight)
                        {
                            Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x - attack.projectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y + attack.projectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }

                    }
                    break;
                case "Cieling":
                    {
                        if (faceRight)
                        {
                            Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x - attack.projectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - attack.projectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }
                        if (!faceRight)
                        {
                            Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x + attack.projectile.GetComponent<Projectile>().launchingPoint.x, transform.position.y - attack.projectile.GetComponent<Projectile>().launchingPoint.y), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }

                    }
                    break;
                case "Right":
                    {
                        if (faceRight)
                        {
                            Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x + attack.projectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + attack.projectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }
                        if (!faceRight)
                        {
                            Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x - attack.projectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + attack.projectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }
                    }
                    break;
                case "Left":
                    {
                        if (faceRight)
                        {
                            Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x - attack.projectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y - attack.projectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }
                        if (!faceRight)
                        {
                            Instantiate<GameObject>(attack.projectile, new Vector2(transform.position.x + attack.projectile.GetComponent<Projectile>().launchingPoint.y, transform.position.y + attack.projectile.GetComponent<Projectile>().launchingPoint.x), transform.rotation, GetComponentInParent<PlayerTracker>().transform);
                        }
                    }
                    break;
            }
            storedProjectile = null;
        }
    }
}
