using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerScript : MonoBehaviour
{
    [Header("Components")]
    public StateMachine statemachine;
    public Animator anim;
    protected Player player;
     public Fighter fighter;
    public int playerID;
    public Rigidbody2D rigidBody;
    public Transform LeapAim;
    [SerializeField] protected LayerMask floor;
    [SerializeField] protected LayerMask foe;
    public LineRenderer lineRenderer;
   public float aimRange;
    public GravAttractor Target;
    public string sTarget;
    public GravAttractor storedTarget;
    public Vector2 LeapTarget;
    public float aim;
    public float aimExtend = 0.25f;
    public GameObject Shaker;
    protected float lowJumpMultiplier = 2f;
    public GameObject storedProjectile;
    public GameObject Shield;
    public Transform GroundCheck;
    public float checkRadius = 0.2f;
    public float leapLimit= 3;
    public float proneTimer;

    [Header("Controls")]
    public BasicAttack attack;
    public bool isPlayer; //if false, it is an AI player
    protected bool leaping; //is it in mid-leap?
    [SerializeField] protected bool AimingRight = true;
    public bool faceRight = true;
    public float Horiz;
    public float Vert;
    public bool BasicTech;
    public bool SpecialTech;
    public bool SpecialTechRelease;
    public bool CelestialTech;
    public bool LeapPrep;
    public bool LeapRelease;
    public bool canMove = true;
    public bool grounded;
    public bool action = false;
    public float actionCooldown = -0.5f;
    public bool jump;
    public bool isBlocking;
    public float jumpDelay = 0.25f;
    public float jumpTimer;
    public bool canInput;
    public bool isMoving;
    public int jumpCount = 0;
    public bool jumpLong;
    public float leapCooldown = 3f;
    public bool isDamaged = false;
    public bool cancelled = false;
    public bool dodging = false;
    public int attackingWith;
    public int currentCombo = 0;
    public int attackCharge = 0;
    protected float lastComboTime = 0f;
    protected float maxComboDelay = 0.9f;
    public float localHoriz;
    public float localVert;
    public bool stunned;
    public bool doneStun;
    public bool slam;
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
    protected void Awake()
    {
        anim = GetComponent<Animator>();
        Shaker = GameObject.FindGameObjectWithTag("Shake");
        maxGrip = fighter.weight;
        HP = fighter.HitPoints;
        jumpHeight = fighter.jumpHeight;
        moveSpeed = fighter.speed;
        maxJumpCount = fighter.jumpCount;
        rigidBody = GetComponent<Rigidbody2D>();
        Target = GameObject.FindGameObjectWithTag("Floor").GetComponent<GravAttractor>();
        Shield = GetComponentInChildren<Barrier>(true).gameObject;
        if (isPlayer)
        {
            player = ReInput.players.GetPlayer(playerID); //The player controlling this fighter
        }
        gameObject.name = fighter.name.ToString() + playerID;
        statemachine = new StateMachine();
        var flinch = new Flinch(this);
        var walk = new Walk(this);
        var run = new Run(this);
        var idle = new Idle(this);
        var jump = new Jump(this);
        var groundattack = new GroundAttack(this, attack);
        var airattack = new AirAttack(this, attack);
        var stun = new Stun(this);
        var leapprep = new LeapPrep(this);
        var blocking = new Blocking(this);
        var dodge = new AirDodge(this);
        var leap = new Leaping(this);
        var prone = new Prone(this);
        At(dodge, idle, landed());
        At(flinch, idle, stunless());
        At(run, idle, stop());
        At(walk, idle, stop());
        At(idle, jump, jumping());
        At(idle, jump, unground());
        At(jump, jump, jumping());
        At(walk, jump, unground());
        At(run, jump, unground());
        At(walk, jump, jumping());
        At(run, jump, jumping());
        At(walk, run, running());
       At(run, walk, walking());
        At(idle, walk, walking());
        At(idle, run, running());
        At(idle, groundattack, offensive());
        At(walk, groundattack, offensive());
        At(run, groundattack, offensive());
        At(jump, airattack, offensive());
        At(groundattack, idle, unoffensive());
        At(airattack, jump, unoffensive());
        At(airattack, idle, landed());
        At(jump, airattack, offensive());
        At(jump, idle, landed());
        At(jump, dodge, guard());
        At(blocking, stun, pierce());
        At(idle, blocking, guard());
        At(walk, blocking, guard());
        At(run, blocking, guard());
        At(leapprep, idle, cancelleap());
        At(leapprep, leap, gravityChange());
        At(blocking, idle, unguard());
        At(leap, idle, landed());
        At(prone, idle, stunless());
        statemachine.AddAnyTransition(leapprep, () => LeapPrep && leapCooldown <= 0);
        statemachine.AddAnyTransition(stun, () => stunned);
        statemachine.AddAnyTransition(flinch, () => isDamaged);
        statemachine.AddAnyTransition(prone, () => slam);
        Func<bool> stunless() => () => doneStun == true; 
        Func<bool> walking() => () => speed > 0.3 && speed < 0.7;
        Func<bool> running() => () => speed > 0.7;
        Func<bool> stop() => () => speed < 0.3;
        Func<bool> jumping() => () => (jumpTimer > Time.time) && (maxJumpCount > jumpCount) && !action && actionCooldown > 0;
        Func<bool> offensive() => () => attack != null && actionCooldown > 0;
        Func<bool> unoffensive() => () => attack == null; 
        Func<bool> landed() => () => grounded;
        Func<bool> pierce() => () => Guard <= 0;
        Func<bool> guard() => () => isBlocking && actionCooldown > 0;
        Func<bool> unguard() => () => !isBlocking;
        Func<bool> gravityChange() => () => LeapRelease;
        Func<bool> cancelleap() => () => cancelled;
        Func<bool> unground() => () => !grounded;
        statemachine.SetState(idle);
    }

    // Start is called before the first frame update
    protected void Start()
    {
        canInput = true;
    }

    // Update is called once per frame
    public virtual void Update()
    {

        GetComponent<GravBody>().attractor = Target;
        grounded = Physics2D.OverlapCircle(GroundCheck.position, checkRadius, floor);
        if (isPlayer)
        {
            GetPlayerInput();
        }
        else
        {
            return;
        }
        speed = Mathf.Abs(localHoriz);
        if ((BasicTech && actionCooldown > 0) && grounded)
        {
            if (localHoriz <= 0.3 && localHoriz >= -0.3 && localVert == 0)
            {
                if (currentCombo < 1)
                {
                    Attack(0);
                }
                else
                {
                    Attack(1);
                }
            }
            if (localHoriz > 0.75 || localHoriz < -0.75)
            {
                Attack(2);
            }
            if (localVert < -0.75)
            {
                Attack(3);
            }
            if (localVert > 0.75)
            {
                Attack(4);
            }
        }
            if ((BasicTech && actionCooldown > 0) && !grounded)
            {

                if (localHoriz == 0 && localVert == 0)
                {
                    Attack(5);
                    actionCooldown = -attack.recharge;
                    canMove = false;
                    currentCombo++;

                }
                if (faceRight)
                {
                    if (localHoriz > 0.75)
                    {
                        Attack(6);
                    }

                    if (localHoriz < -0.75)
                    {
                        Attack(9);
                    }
                }
                if (!faceRight)
                {
                    if (localHoriz > 0.75)
                    {
                        Attack(9);
                    }

                    if (localHoriz < -0.75)
                    {
                        Attack(6);
                    }
                }
                if (localVert < -0.75)
                {
                    Attack(7);
                }
                if (localVert > 0.75)
                {
                    Attack(8);

                }
            }
      
            if ((SpecialTech && actionCooldown > 0) && grounded)
            {
                if (localHoriz <= 0.3 && localHoriz >= -0.3 && localVert == 0)
                {
                    Attack(10);
                }

                if (localHoriz > 0.75 || localHoriz < -0.75)
                {
                    Attack(12);


                }


                if (localVert < -0.75)
                {
                    Attack(14);

                }
                if (localVert > 0.75)
                {
                    Attack(16);

                }

            }
       
        if (SpecialTechRelease)
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

            actionCooldown += Time.deltaTime;

            if (Time.time - lastComboTime >= maxComboDelay)
            {
                currentCombo = 0;
            }
            if (jump)
            {
                jumpTimer = Time.time + jumpDelay;
            }
            sTarget = Target.gameObject.tag;
            statemachine.Tick();
        }
    
    void At(IState to, IState from, Func<bool> condition) => statemachine.AddTransition(to, from, condition);
    public virtual void FixedUpdate()
    {

       //if  ((jumpTimer > Time.time) && (maxJumpCount > jumpCount) && canMove && !action && actionCooldown > 0)            
         //   {
           //     Jump();
            // }
        
       
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
        //anim.SetBool("damaged", isDamaged);
        leapCooldown -= Time.deltaTime;
        if (Guard >= 100)
        {
            Guard = 100;
        }
        if (Guard < 0)
        {
            Guard = 0;
        }
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
    public void GetPlayerInput()
    {
        Horiz = player.GetAxis("Horizontal");
        Vert = player.GetAxis("Vertical");
        jump = player.GetButtonDown("Jump");
        jumpLong = player.GetButton("Jump");
        isBlocking = player.GetButton("Block");
        BasicTech = player.GetButtonDown("Attack");
        SpecialTech = player.GetButtonDown("Special");
        SpecialTechRelease = player.GetButtonUp("Special");
        LeapPrep = player.GetButton("Leap");
        LeapRelease = player.GetButtonUp("Leap");
        if ((player.GetButtonDown("Attack") && player.GetButtonDown("Special"))){
            CelestialTech = true;
        }
        if ((!player.GetButtonDown("Attack") || !player.GetButtonDown("Special"))){
            CelestialTech = false; }
    }
    public void unAttack()
    {
        attack = null;
        storedProjectile = null;
        canMove = true;
        action = false;
        attackCharge = 0;
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
    public void Land()
    {
        dodging = false;
        canInput = true;
        canMove = true;
        action = false;
    }
    public Vector2 AimLeap()
    {
        if (LeapAim.localPosition.y != .75f)
        {
            LeapAim.localPosition = new Vector3(0, .75f, 0);
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

                LeapAim.localEulerAngles = new Vector3(LeapAim.localRotation.x, LeapAim.localRotation.y, -aim);
            }
                return LeapTarget;
            }
            else
            {
                storedTarget = Target;
                return transform.position;
            }
        }
    public void Attack(int tech)
    {
            attack = fighter.techniques[tech];
            anim.Play(attack.animationName);
            lastComboTime = Time.time;
        actionCooldown = attack.recharge;
    }
    public void OnGround()
    {
        jumpCount = 0;
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
    public void OffGround()
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
                if (rigidBody.velocity.y > 0 && !jumpLong)
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
                if (rigidBody.velocity.y < 0 && !jumpLong)
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
                if (rigidBody.velocity.x > 0 && !jumpLong)
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
                if (rigidBody.velocity.x < 0 && !jumpLong)
                {
                    rigidBody.velocity += Vector2.left * GetComponent<GravBody>().gravity * (lowJumpMultiplier - 1) * Time.deltaTime;
                }
                break;
            default:

                break;
        }
    }
    public void Dodge()
    {
        dodging = !dodging;
        if (dodging)
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
    }
    public void Jump()
    {
       jumpCount += 1;

        switch (GetComponent<GravBody>().attractor.gameObject.tag)
        {
            case "Floor":
                rigidBody.velocity = new Vector2(rigidBody.velocity.x * Time.deltaTime, jumpHeight * Time.deltaTime);
                break;
            case "Cieling":
                rigidBody.velocity = new Vector2(rigidBody.velocity.x * Time.deltaTime, -jumpHeight * Time.deltaTime);
                break;
            case "Left":
                rigidBody.velocity = new Vector2(jumpHeight  * Time.deltaTime, rigidBody.velocity.y * Time.deltaTime);
                break;
            case "Right":
                rigidBody.velocity = new Vector2(-jumpHeight  * Time.deltaTime, rigidBody.velocity.y * Time.deltaTime);
                break;
            default:
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpHeight);
                break;
        }
        jumpTimer = 0;

    }
    public void Leap()
    {
        if (((Target.tag == "Floor") && (storedTarget.tag == "Cieling")) || ((Target.tag == "Cieling") && (storedTarget.tag == "Floor")) || ((Target.tag == "Left") && (storedTarget.tag == "Right")) || ((Target.tag == "Right") && (storedTarget.tag == "Left")))
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
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "HitBox")
        {

            if (!collision.GetComponent<AttackController>().isProjectile || (collision.GetComponent<AttackController>().isProjectile && (collision.gameObject.GetComponent<AttackController>().owner != playerID)))
            {
                if(!Shield.activeSelf  && !dodging)
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

                        rigidBody.AddForceAtPosition(knock * source * back, collision.transform.localPosition * Time.deltaTime, ForceMode2D.Force);
                    }
                    if (!collision.gameObject.GetComponent<AttackController>().flinchless)
                    {
                        isDamaged = true;
                    }
                    damage += collision.gameObject.GetComponent<AttackController>().damagePower;
                    gripLoss += collision.gameObject.GetComponent<AttackController>().gripLoss;
                    if (gripLoss >= maxGrip)
                    {
                        Launch(knock * source);
                    }
                }
                if (Shield.activeInHierarchy)
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
                GetComponentInParent<PlayerTracker>().shrinkTimer = GetComponentInParent<PlayerTracker>().shrinkTimerMax;
                GetComponentInParent<PlayerTracker>().SetHealth(GetComponentInParent<PlayerTracker>().GetHealthNormalized());
            }
            else return;
        }
    }
    public void ShakeOff()
    {
        doneStun = true;
    }
    public void GuardBreak()
    {
        vulnerable = true;
    }
    public void Launch(Vector2 dir)
    {
        
        slam = true;
        canInput = false;
        canMove = false;
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, dir, 10000, floor);
        if (hitInfo)
        {
            GravAttractor wall = hitInfo.transform.GetComponent<GravAttractor>();
            storedTarget = wall; ;
            if (((Target.tag == "Floor") && (storedTarget.tag == "Cieling")) || ((Target.tag == "Cieling") && (storedTarget.tag == "Floor")) || ((Target.tag == "Left") && (storedTarget.tag == "Right")) || ((Target.tag == "Right") && (storedTarget.tag == "Left")))
            {
                faceRight = !faceRight;

            }
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
}
