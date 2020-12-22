using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Object = System.Object;
public class StateMachine
{
    private IState _currentState;

    private Dictionary<Type, List<Transition>> _transitions = new Dictionary<Type, List<Transition>>();
    private List<Transition> _currentTransitions = new List<Transition>();
    private List<Transition> _anyTransitions = new List<Transition>();

    private static List<Transition> EmptyTransitions = new List<Transition>(0);

    public void Tick()
    {
        var transition = GetTransition();
        if (transition != null)
            SetState(transition.To);

        _currentState?.Tick();
    }

    public void SetState(IState state)
    {
        if (state == _currentState)
            return;

        _currentState?.OnExit();
        _currentState = state;

        _transitions.TryGetValue(_currentState.GetType(), out _currentTransitions);
        if (_currentTransitions == null)
            _currentTransitions = EmptyTransitions;

        _currentState.OnEnter();
    }

    public void AddTransition(IState from, IState to, Func<bool> predicate)
    {
        if (_transitions.TryGetValue(from.GetType(), out var transitions) == false)
        {
            transitions = new List<Transition>();
            _transitions[from.GetType()] = transitions;
        }

        transitions.Add(new Transition(to, predicate));
    }

    public void AddAnyTransition(IState state, Func<bool> predicate)
    {
        _anyTransitions.Add(new Transition(state, predicate));
    }

    private class Transition
   {
      public Func<bool> Condition {get; }
      public IState To { get; }

      public Transition(IState to, Func<bool> condition)
      {
         To = to;
         Condition = condition;
      }
   }
    private Transition GetTransition()
    {
        foreach (var transition in _anyTransitions)
            if (transition.Condition())
                return transition;

        foreach (var transition in _currentTransitions)
            if (transition.Condition())
                return transition;

        return null;
    }
}
public class Prone : IState
{
    readonly PlayerScript _player;
    Animator anim;
    public Prone(PlayerScript ps)
        {
        _player = ps;
        anim = ps.anim;
    }
    public void OnEnter()
    {
        anim.SetBool("knocked out", true);
    }
    public void Tick()
    {
        _player.canInput = false;
        _player.canMove = false; 
        if (_player.grounded)
        {
            _player.OnGround();
        }
        else
        {
            _player.OffGround();
        }
        anim.SetBool("grounded", _player.grounded);
        _player.vulnerable = true;

    }
    public void OnExit() { }

}
public class Blocking : IState
{
    readonly PlayerScript _player;
    Animator anim;

    public Blocking(PlayerScript ps)
    {
        _player = ps;
        anim = ps.anim;
    }
    public  void OnEnter()
    {
        anim.SetBool("grounded", true);
        anim.SetBool("Guard", true);
        anim.SetFloat("speed", 0);
        anim.SetBool("damaged", false);
    }
    public void Tick()
    {
        _player.Shield.SetActive(true);
        _player.Guard -= 0.1f;
    }
    public void OnExit()
    {
        anim.SetBool("Guard", false);
 _player.Shield.SetActive(false);
    }
}
public class AirDodge : IState
{
    readonly PlayerScript _player;
    Animator anim;
    public AirDodge(PlayerScript ps)
    {
        _player = ps;
        anim = ps.anim;
    }
    public void OnEnter()
    {
        anim.SetBool("grounded", false);
        anim.SetTrigger("Dodge");
        anim.SetBool("damaged", false);
    }
    public void Tick()
    {
        _player.vulnerable = false;
        _player.Dodge();
    }
    public void OnExit()
    {
    }
}
public class AirAttack : IState
{
    readonly PlayerScript _player;
    Animator anim;
    BasicAttack a;
    public AirAttack(PlayerScript ps, BasicAttack attack)
    {
        a = attack;
        _player = ps;
        anim = ps.anim;
    }
    public void OnEnter()
    {
        anim.SetBool("grounded", false);
        anim.SetBool("damaged", false);
    }
    public void Tick()
    {
        if (_player.Guard < 100)
        {
            _player.Guard += 0.4f;
        }
        if (_player.attack.projectile != null)
        {
            _player.storedProjectile = _player.attack.projectile;
        }
        else return;

        if (!_player.attack.chargable && !_player.attack.delay)
        {
            if (_player.storedProjectile != null)
            {
                switch (_player.Target.gameObject.tag)
                {
                    case "Floor":
                        {
                            if (_player.faceRight)
                            {
                                GameObject projectile = GameObject.Instantiate<GameObject>(_player.attack.projectile, new Vector2(_player.transform.position.x + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x, _player.transform.position.y + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;
                            }
                            if (!_player.faceRight)
                            {
                                GameObject projectile = GameObject.Instantiate<GameObject>(_player.attack.projectile, new Vector2(_player.transform.position.x - _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x, _player.transform.position.y + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;
                            }

                        }
                        break;
                    case "Cieling":
                        {
                            if (_player.faceRight)
                            {
                                GameObject projectile = GameObject.Instantiate<GameObject>(_player.attack.projectile, new Vector2(_player.transform.position.x - _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x, _player.transform.position.y - _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;
                            }
                            if (!_player.faceRight)
                            {
                                GameObject projectile = GameObject.Instantiate<GameObject>(_player.attack.projectile, new Vector2(_player.transform.position.x + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x, _player.transform.position.y - _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;
                            }

                        }
                        break;
                    case "Right":
                        {
                            if (_player.faceRight)
                            {
                                GameObject projectile = GameObject.Instantiate<GameObject>(_player.attack.projectile, new Vector2(_player.transform.position.x + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y, _player.transform.position.y + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;
                            }
                            if (!_player.faceRight)
                            {
                                GameObject projectile = GameObject.Instantiate<GameObject>(_player.attack.projectile, new Vector2(_player.transform.position.x - _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y, _player.transform.position.y + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;
                            }
                        }
                        break;
                    case "Left":
                        {
                            if (_player.faceRight)
                            {
                                GameObject projectile = GameObject.Instantiate<GameObject>(_player.attack.projectile, new Vector2(_player.transform.position.x - _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y, _player.transform.position.y - _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;
                            }
                            if (!_player.faceRight)
                            {
                                GameObject projectile = GameObject.Instantiate<GameObject>(_player.attack.projectile, new Vector2(_player.transform.position.x + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y, _player.transform.position.y + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;
                            }
                        }
                        break;
                }
                _player.storedProjectile = null;
            }
            else
            {
                _player.currentCombo++;
                _player.actionCooldown = -_player.attack.recharge;
            }

        }
        else
        {
            if (_player.isDamaged)
            {
                _player.unAttack();
            }

        }
    }
    public void OnExit()
    {
    }
}


public class GroundAttack : IState
{
    readonly PlayerScript _player;
    Animator anim;
    BasicAttack a;
    public GroundAttack(PlayerScript ps, BasicAttack attack)
    {
        a = attack;
        _player = ps;
        anim = ps.anim;
    }
    public void OnEnter()
    {
        anim.SetBool("grounded", true);
        anim.SetBool("damaged", false);
        _player.OnGround();
        _player.canMove = false;
    }
    public void Tick()
    {
        if (_player.Guard < 100)
        {
            _player.Guard += 0.4f;
        }
        if (_player.attack.projectile != null)
        {
            _player.storedProjectile = _player.attack.projectile;
        }
        else return;

        if (!_player.attack.chargable && !_player.attack.delay)
        {
            if (_player.storedProjectile != null)
            {
                switch (_player.Target.gameObject.tag)
                {
                    case "Floor":
                        {
                            if (_player.faceRight)
                            {
                                GameObject projectile = GameObject.Instantiate<GameObject>(_player.attack.projectile, new Vector2(_player.transform.position.x + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x, _player.transform.position.y + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;

                            }
                            if (!_player.faceRight)
                            {
                                GameObject projectile = GameObject.Instantiate<GameObject>(_player.attack.projectile, new Vector2(_player.transform.position.x - _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x, _player.transform.position.y + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;

                            }

                        }
                        break;
                    case "Cieling":
                        {
                            if (_player.faceRight)
                            {
                                GameObject projectile = GameObject.Instantiate<GameObject>(_player.attack.projectile, new Vector2(_player.transform.position.x - _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x, _player.transform.position.y - _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;
                            }
                            if (!_player.faceRight)
                            {
                                GameObject projectile =GameObject.Instantiate<GameObject>(_player.attack.projectile, new Vector2(_player.transform.position.x + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x, _player.transform.position.y - _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;
                            }

                        }
                        break;
                    case "Right":
                        {
                            if (_player.faceRight)
                            {
                               GameObject projectile = GameObject.Instantiate<GameObject> (_player.attack.projectile, new Vector2(_player.transform.position.x + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y, _player.transform.position.y + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;
                            }
                            if (!_player.faceRight)
                            {
                                GameObject projectile= GameObject.Instantiate<GameObject>(_player.attack.projectile, new Vector2(_player.transform.position.x - _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y, _player.transform.position.y + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;
                            }
                        }
                        break;
                    case "Left":
                        {
                            if (_player.faceRight)
                            {
                                GameObject projectile = GameObject.Instantiate<GameObject>(_player.attack.projectile, new Vector2(_player.transform.position.x - _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y, _player.transform.position.y - _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;
                            }
                            if (!_player.faceRight)
                            {
                                GameObject projectile = GameObject.Instantiate<GameObject>(_player.attack.projectile, new Vector2(_player.transform.position.x + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.y, _player.transform.position.y + _player.attack.projectile.GetComponent<Projectile>().launchingPoint.x), _player.transform.rotation, _player.GetComponentInParent<PlayerTracker>().transform);
                                projectile.GetComponent<AttackController>().owner = _player.playerID;
                            }
                        }
                        break;
                }                                

                _player.storedProjectile = null;
            }
            else
            {
                _player.currentCombo++;
                _player.actionCooldown = -_player.attack.recharge;
            }

        }
        else
        {
            if (_player.isDamaged)
            {
                _player.unAttack();
            }

        }
    }
    public  void OnExit()
    {
        _player.currentCombo++;
    }
}


public class LeapPrep : IState
{
    readonly PlayerScript _player;
    Animator anim;
    public LeapPrep(PlayerScript ps)
    {
        _player = ps;
        anim = ps.anim;
    }
    public void OnEnter()
    {
        anim.SetBool("grounded", true);
        anim.SetFloat("speed", 0);
        anim.SetBool("leap", true);
        _player.AimLeap();
    }
    public void Tick()
    {
        if (_player.Guard < 100)
        {
            _player.Guard += 0.4f;
        }
        _player.aim = Mathf.PingPong(_player.aimSpeed * Time.fixedTime, _player.aimRange);
        _player.LeapTarget = _player.AimLeap();
    }
    public void OnExit()
    {
        _player.aim = 0;
        anim.SetBool("leap", false);

        _player.lineRenderer.enabled = false;

    }
}

public class Leaping : IState
{
    readonly PlayerScript _player;
    Animator anim;
    public Leaping(PlayerScript ps)
    {
        _player = ps;
        anim = ps.anim;
    }
    public void OnEnter()
    {
    }
    public void Tick()
    {
        if (_player.Guard < 100)
        {
            _player.Guard += 0.4f;
        }
        _player.Leap();
    }
    public void OnExit()
    {
        _player.leapCooldown = _player.leapLimit;


    }
}
public class Stun : IState
{
    readonly PlayerScript _player;
    Animator anim;

    public Stun(PlayerScript ps)
    {
        _player = ps;
        anim = ps.anim;
    }
    public void OnEnter()
    {
        anim.SetBool("grounded", true);
        anim.SetFloat("speed", 0);
        anim.SetTrigger("Broken");
    }
    public void Tick()
    {
        anim.SetBool("grounded", true);
        anim.SetFloat("speed", 0);
        _player.OnGround();
        _player.Shield.SetActive(false);
        _player.vulnerable = true;
    }
    public void OnExit()
    {

    }
}
public class Flinch : IState
{
    readonly PlayerScript _player;
    Animator anim;

    public Flinch(PlayerScript ps)
    {
        _player = ps;
        anim = ps.anim;
    }
    public void OnEnter()
    {
        anim.SetBool("damaged", true);
    }
    public void Tick()
    {
        if (_player.grounded)
        {
            _player.OnGround();
        }
        else
        {
            _player.OffGround();
        }
        _player.isDamaged = true;
        _player.canMove = false;
    }
    public  void OnExit()
    {
    }
}
public class Jump : IState
{
    readonly PlayerScript _player;
    Animator anim;
    public Jump(PlayerScript ps)
    {
        _player = ps;
        anim = ps.anim;
    }
    public void OnEnter()
    {

        anim.SetBool("grounded", false);
        anim.SetBool("damaged", false);

    }
    public void Tick()
    {
        _player.OffGround(); Vector2 mover;
        switch (_player.gameObject.GetComponent<GravBody>().attractor.gameObject.tag)
        {
            case "Floor":

                mover = new Vector2(_player.Horiz * _player.moveSpeed * Time.deltaTime, 0);


                if ((_player.Horiz > 0))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }
                if ((_player.Horiz < 0))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }
                if (_player.Horiz == 0)
                {
                    _player.isMoving = false;
                    _player.speed = 0;
                }
                break;
            case "Cieling":

                mover = new Vector2(_player.Horiz * _player.moveSpeed * Time.deltaTime, 0);

                if ((_player.Horiz < 0))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }
                if (_player.Horiz > 0)
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }
                if (_player.Horiz == 0)
                {
                    _player.isMoving = false;
                    _player.speed = 0;
                }
                break;
            case "Left":

                mover = new Vector2(0, _player.Vert * _player.moveSpeed * Time.deltaTime);

                if ((_player.Vert < 0))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Vert);
                }
                if ((_player.Vert > 0))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Vert);
                }
                if (_player.Vert == 0)
                {
                    _player.isMoving = false;
                    _player.speed = 0;

                }
                break;
            case "Right":

                mover = new Vector2(0, _player.Vert * _player.moveSpeed * Time.deltaTime);

                if ((_player.Vert > 0))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Vert);
                }
                if ((_player.Vert < 0))
                {
                    _player.speed = Mathf.Abs(_player.Vert);
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Vert);
                }
                if (_player.Vert == 0)
                {
                    _player.isMoving = false;
                    _player.speed = 0;

                }
                break;
            default:
                mover = new Vector2(_player.Horiz * _player.moveSpeed * Time.deltaTime, 0);
                if ((_player.Horiz > 0))
                {
                    _player.speed = Mathf.Abs(_player.Horiz);
                    _player.faceRight = true;
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }
                if ((_player.Horiz < 0))
                {
                    _player.faceRight = false;
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }
                if (_player.Horiz == 0)
                {
                    _player.isMoving = false;
                    _player.speed = 0;

                }
                break;
        }

        _player.rigidBody.AddForce(mover * 25);

    }
    public void OnExit()
    {
    }
}
public class Idle : IState
{
    readonly PlayerScript _player;
    Animator anim;

    public Idle (PlayerScript ps)
    {
        _player = ps;
        anim = ps.anim;
    }
    public void OnEnter()
    {
        _player.vulnerable = false;
        _player.Shield.SetActive(false);
        _player.doneStun = false;
        anim.SetBool("grounded", true);
        anim.SetBool("Guard", false);
        anim.SetFloat("speed", 0f);
        anim.SetBool("damaged", false);
    }
    public void Tick()
    {
        _player.OnGround();
        if (_player.Guard < 100)
        {
            _player.Guard += 0.4f;
        }
        switch (_player.gameObject.GetComponent<GravBody>().attractor.gameObject.tag)
            {
                case "Floor":
                    if ((_player.faceRight && _player.Horiz < 0) || (!_player.faceRight && _player.Horiz > 0))
                    {
                        _player.Flip();
                    }
                    _player.localHoriz = _player.Horiz;
                    _player.localVert = _player.Vert;
                    break;
                case "Cieling":
                    if ((_player.faceRight && _player.Horiz > 0) || (!_player.faceRight && _player.Horiz < 0))
                    {
                        _player.Flip();
                    }
                    _player.localHoriz = _player.Horiz;
                    _player.localVert = -_player.Vert;
                    break;
                case "Left":
                    if ((_player.faceRight && _player.Vert > 0) || (!_player.faceRight && _player.Vert < 0))
                    {
                        _player.Flip();
                    }
                    _player.localHoriz = _player.Vert;
                    _player.localVert = _player.Horiz;
                    break;
                case "Right":
                    if ((_player.faceRight && _player.Vert < 0) || (!_player.faceRight && _player.Vert > 0))
                    {
                        _player.Flip();
                    }
                    _player.localHoriz = _player.Vert;
                    _player.localVert = -_player.Horiz;
                    break;
                default:
                    if ((_player.faceRight && _player.Horiz < 0) || (!_player.faceRight && _player.Horiz > 0))
                    {
                        _player.Flip();
                    }
                    _player.localHoriz = _player.Horiz;
                    _player.localVert = _player.Vert;
                    break;
            }
      
    }
    public void OnExit()
    {
    }
}
public class Walk : IState
{
    readonly PlayerScript _player;
    Animator anim;
    public float walkSpeed;
    public Walk(PlayerScript ps)
    {
        _player = ps;
       walkSpeed = ps.speed;
        anim = ps.anim;
    }
    public void OnEnter()
    {
        _player.vulnerable = false;
        _player.Shield.SetActive(false);
        anim.SetBool("grounded", true);
        anim.SetFloat("speed", walkSpeed);
        anim.SetBool("damaged", false);
    }
    public void Tick()
    {
        anim.SetFloat("speed", _player.speed);
        anim.SetBool("grounded", true);

        Vector2 mover; if (_player.Guard < 100)
        {
            _player.Guard += 0.4f;
        }
        _player.OnGround();
        switch (_player.gameObject.GetComponent<GravBody>().attractor.gameObject.tag)
        {
            case "Floor":

                mover = new Vector2(_player.Horiz * _player.moveSpeed * Time.deltaTime, 0);


                if ((_player.Horiz > 0) && (!_player.action))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }
                if ((_player.Horiz < 0) && (!_player.action))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }
                
                break;
            case "Cieling":

                mover = new Vector2(_player.Horiz * _player.moveSpeed * Time.deltaTime, 0);

                if ((_player.Horiz < 0) && (!_player.action))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }
                if ((_player.Horiz > 0) && (!_player.action))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }
                
                break;
            case "Left":

                mover = new Vector2(0, _player.Vert * _player.moveSpeed * Time.deltaTime);

                if ((_player.Vert < 0) && (!_player.action))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Vert);
                }
                if ((_player.Vert > 0) && (!_player.action))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Vert);
                }
                break;
            case "Right":

                mover = new Vector2(0, _player.Vert * _player.moveSpeed * Time.deltaTime);

                if ((_player.Vert > 0) && (!_player.action))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Vert);
                }
                if ((_player.Vert < 0) && (!_player.action))
                {
                    _player.speed = Mathf.Abs(_player.Vert);
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Vert);
                }
                
                break;
            default:
                mover = new Vector2(_player.Horiz * _player.moveSpeed * Time.deltaTime, 0);
                if ((_player.Horiz > 0) && (!_player.action))
                {
                    _player.speed = Mathf.Abs(_player.Horiz);
                    _player.isMoving = true;
                }
                if ((_player.Horiz < 0) && (!_player.action))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }
                break;
        }

        _player.rigidBody.AddForce(mover * 100);
        switch (_player.gameObject.GetComponent<GravBody>().attractor.gameObject.tag)
            {
                case "Floor":
                    if ((_player.faceRight && _player.Horiz < 0) || (!_player.faceRight && _player.Horiz > 0))
                    {
                        _player.Flip();
                    }
                    _player.localHoriz = _player.Horiz;
                    _player.localVert = _player.Vert;
                    break;
                case "Cieling":
                    if ((_player.faceRight && _player.Horiz > 0) || (!_player.faceRight && _player.Horiz < 0))
                    {
                        _player.Flip();
                    }
                    _player.localHoriz = _player.Horiz;
                    _player.localVert = -_player.Vert;
                    break;
                case "Left":
                    if ((_player.faceRight && _player.Vert > 0) || (!_player.faceRight && _player.Vert < 0))
                    {
                        _player.Flip();
                    }
                    _player.localHoriz = _player.Vert;
                    _player.localVert = _player.Horiz;
                    break;
                case "Right":
                    if ((_player.faceRight && _player.Vert < 0) || (!_player.faceRight && _player.Vert > 0))
                    {
                        _player.Flip();
                    }
                    _player.localHoriz = _player.Vert;
                    _player.localVert = -_player.Horiz;
                    break;
                default:
                    if ((_player.faceRight && _player.Horiz < 0) || (!_player.faceRight && _player.Horiz > 0))
                    {
                        _player.Flip();
                    }
                    _player.localHoriz = _player.Horiz;
                    _player.localVert = _player.Vert;
                    break;
            }
        
    }
    public void OnExit()
    {
    }
}
public class Run : IState
{
    readonly PlayerScript _player;
    Animator anim;
    public float runSpeed;
    public Run(PlayerScript ps)
    {
        _player = ps;
       runSpeed = ps.speed;
        anim = ps.anim;
    }
    public void OnEnter()
    {
        _player.vulnerable = false;
        _player.Shield.SetActive(false);
        anim.SetBool("grounded", true);
        anim.SetFloat("speed", _player.speed);
        anim.SetBool("damaged", false);
    }
    public  void Tick()
    {
        anim.SetFloat("speed", _player.speed);

        if (_player.Guard < 100)
        {
            _player.Guard += 0.4f;
        }
        _player.OnGround();
        Vector2 mover;
        switch (_player.gameObject.GetComponent<GravBody>().attractor.gameObject.tag)
        {
            case "Floor":

                mover = new Vector2(_player.Horiz * _player.moveSpeed * Time.deltaTime, 0);


                if ((_player.Horiz > 0) && (!_player.action))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }
                if ((_player.Horiz < 0) && (!_player.action))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }

                break;
            case "Cieling":

                mover = new Vector2(_player.Horiz * _player.moveSpeed * Time.deltaTime, 0);

                if ((_player.Horiz < 0) && (!_player.action))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }
                if ((_player.Horiz > 0) && (!_player.action))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }

                break;
            case "Left":

                mover = new Vector2(0, _player.Vert * _player.moveSpeed * Time.deltaTime);

                if ((_player.Vert < 0) && (!_player.action))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Vert);
                }
                if ((_player.Vert > 0) && (!_player.action))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Vert);
                }
                break;
            case "Right":

                mover = new Vector2(0, _player.Vert * _player.moveSpeed * Time.deltaTime);

                if ((_player.Vert > 0) && (!_player.action))
                {
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Vert);
                }
                if ((_player.Vert < 0) && (!_player.action))
                {
                    _player.speed = Mathf.Abs(_player.Vert);
                    _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Vert);
                }

                break;
            default:
                mover = new Vector2(_player.Horiz * _player.moveSpeed * Time.deltaTime, 0);
                if ((_player.Horiz > 0) && (!_player.action))
                {
                    _player.speed = Mathf.Abs(_player.Horiz);
                    _player.isMoving = true;
                }
                if ((_player.Horiz < 0) && (!_player.action))
                {
                   _player.isMoving = true;
                    _player.speed = Mathf.Abs(_player.Horiz);
                }
                break;
        }

        _player.rigidBody.AddForce(mover * 100);
        switch (_player.gameObject.GetComponent<GravBody>().attractor.gameObject.tag)
        {
            case "Floor":
                if ((_player.faceRight && _player.Horiz < 0) || (!_player.faceRight && _player.Horiz > 0))
                {
                    _player.Flip();
                }
                _player.localHoriz = _player.Horiz;
                _player.localVert = _player.Vert;
                break;
            case "Cieling":
                if ((_player.faceRight && _player.Horiz > 0) || (!_player.faceRight && _player.Horiz < 0))
                {
                    _player.Flip();
                }
                _player.localHoriz = _player.Horiz;
                _player.localVert = -_player.Vert;
                break;
            case "Left":
                if ((_player.faceRight && _player.Vert > 0) || (!_player.faceRight && _player.Vert < 0))
                {
                    _player.Flip();
                }
                _player.localHoriz = _player.Vert;
                _player.localVert = _player.Horiz;
                break;
            case "Right":
                if ((_player.faceRight && _player.Vert < 0) || (!_player.faceRight && _player.Vert > 0))
                {
                    _player.Flip();
                }
                _player.localHoriz = _player.Vert;
                _player.localVert = -_player.Horiz;
                break;
            default:
                if ((_player.faceRight && _player.Horiz < 0) || (!_player.faceRight && _player.Horiz > 0))
                {
                    _player.Flip();
                }
                _player.localHoriz = _player.Horiz;
                _player.localVert = _player.Vert;
                break;
        }
        _player.rigidBody.AddForce(mover * 100);
    }
    public void OnExit()
    {
    }
}
