#if UNITY_EDITOR
using System.Collections;
using AlmaGame.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public sealed class AlmaMovementTests
{
    private GameObject _root;
    private GameObject _floor;
    private AlmaMotor2D _motor;
    private Rigidbody2D _body;
    private Animator _animator;
    private readonly WaitForFixedUpdate _fixed = new WaitForFixedUpdate();

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _root = new GameObject("Alma_Test_Fixture");
        _floor = new GameObject("Floor");
        _floor.transform.SetParent(_root.transform);
        _floor.transform.position = new Vector3(1000f, -0.5f);
        _floor.AddComponent<BoxCollider2D>().size = new Vector2(100f, 1f);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/Alma.prefab");
        var player = Object.Instantiate(prefab, new Vector3(1000f, 1.3f), Quaternion.identity, _root.transform);
        player.GetComponent<AlmaInput>().enabled = false;
        _motor = player.GetComponent<AlmaMotor2D>();
        _body = player.GetComponent<Rigidbody2D>();
        _animator = player.GetComponent<Animator>();
        for (int i = 0; i < 15; i++) yield return _fixed;
        Assert.That(_motor.IsGrounded, Is.True, "Fixture should settle on floor.");
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(_root);
        yield return null;
    }

    [UnityTest]
    public IEnumerator RunsBrakesTurnsAndAnimates()
    {
        _motor.SetInput(1f, false, false);
        for (int i = 0; i < 12; i++) yield return _fixed;
        Assert.That(_body.linearVelocity.x, Is.EqualTo(7f).Within(0.15f));
        yield return null;
        Assert.That(_animator.GetCurrentAnimatorStateInfo(0).IsName("Run"), Is.True);
        Sprite first = _motor.GetComponent<SpriteRenderer>().sprite;
        yield return new WaitForSeconds(0.18f);
        Assert.That(_motor.GetComponent<SpriteRenderer>().sprite, Is.Not.EqualTo(first), "Run must have actual sprite frames.");
        _motor.SetInput(-1f, false, false);
        for (int i = 0; i < 15; i++) yield return _fixed;
        Assert.That(_body.linearVelocity.x, Is.LessThan(-6.8f));
        Assert.That(_motor.GetComponent<SpriteRenderer>().flipX, Is.True);
        _motor.SetInput(0f, false, false);
        for (int i = 0; i < 8; i++) yield return _fixed;
        yield return null;
        Assert.That(Mathf.Abs(_body.linearVelocity.x), Is.LessThan(0.05f));
        Assert.That(_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"), Is.True);
    }

    [UnityTest]
    public IEnumerator HeldJumpTransitionsThroughRiseFallAndLandingOnce()
    {
        float start = _body.position.y, peak = start;
        bool sawJump = false, sawFall = false;
        _motor.SetInput(0f, true, true);
        for (int i = 0; i < 70; i++)
        {
            yield return _fixed;
            peak = Mathf.Max(peak, _body.position.y);
            sawJump |= _animator.GetCurrentAnimatorStateInfo(0).IsName("Jump");
            sawFall |= _animator.GetCurrentAnimatorStateInfo(0).IsName("Fall");
        }
        Assert.That(peak - start, Is.InRange(1.35f, 1.65f));
        Assert.That(sawJump && sawFall, Is.True, "Air animation must follow velocity.");
        Assert.That(_motor.IsGrounded, Is.True, "Holding jump must not auto-jump on landing.");
        Assert.That(_body.position.y, Is.EqualTo(start).Within(0.04f));
    }

    [UnityTest]
    public IEnumerator QuickPressBetweenPhysicsStepsProducesShortJump()
    {
        float start = _body.position.y, peak = start;
        _motor.SetInput(0f, true, true);
        _motor.SetInput(0f, false, false);
        for (int i = 0; i < 45; i++)
        {
            yield return _fixed;
            peak = Mathf.Max(peak, _body.position.y);
        }
        Assert.That(peak - start, Is.InRange(0.45f, 0.85f));
        Assert.That(_motor.IsGrounded, Is.True);
    }

    [UnityTest]
    public IEnumerator QueuedJumpJustBeforeLandingIsConsumedOnce()
    {
        _motor.SetInput(0f, true, true);
        for (int i = 0; i < 50; i++)
        {
            yield return _fixed;
            if (_body.linearVelocity.y < -1f && _body.position.y < 1.6f) break;
        }
        Assert.That(_motor.IsGrounded, Is.False);
        _motor.SetInput(0f, true, true);
        bool jumpedAgain = false;
        for (int i = 0; i < 10; i++)
        {
            yield return _fixed;
            jumpedAgain |= _body.linearVelocity.y > 4f;
        }
        Assert.That(jumpedAgain, Is.True, "Buffered input should survive until landing.");
    }

    [UnityTest]
    public IEnumerator CanJumpDuringCoyoteWindowButCannotJumpTwice()
    {
        _floor.SetActive(false);
        for (int i = 0; i < 3; i++) yield return _fixed;
        _motor.SetInput(0f, true, true);
        yield return _fixed;
        yield return _fixed;
        Assert.That(_body.linearVelocity.y, Is.GreaterThan(6f));
        for (int i = 0; i < 10; i++) yield return _fixed;
        float before = _body.linearVelocity.y;
        _motor.SetInput(0f, true, true);
        yield return _fixed;
        yield return _fixed;
        Assert.That(_body.linearVelocity.y, Is.LessThan(before), "Base movement must not grant an extra air jump.");
    }

    [UnityTest]
    public IEnumerator WallContactIsNotGroundAndDoesNotStick()
    {
        var wall = new GameObject("Wall");
        wall.transform.SetParent(_root.transform);
        wall.transform.position = new Vector3(1001.1f, 4f);
        wall.AddComponent<BoxCollider2D>().size = new Vector2(1f, 20f);
        _body.position = new Vector2(1000f, 4f);
        _floor.SetActive(false);
        _motor.SetInput(1f, false, false);
        for (int i = 0; i < 15; i++) yield return _fixed;
        Assert.That(_motor.IsGrounded, Is.False);
        Assert.That(_body.linearVelocity.y, Is.LessThan(-2f));
        _motor.SetInput(1f, true, true);
        yield return _fixed;
        yield return _fixed;
        Assert.That(_body.linearVelocity.y, Is.LessThan(0f));
    }

    [UnityTest]
    public IEnumerator FallingBelowBoundaryRespawnsAtInitialPosition()
    {
        _body.position = new Vector2(1005f, -15f);
        _body.linearVelocity = new Vector2(4f, -19f);
        yield return _fixed;
        yield return _fixed;
        Assert.That(_body.position.x, Is.EqualTo(1000f).Within(0.05f));
        Assert.That(_body.position.y, Is.GreaterThan(1f));
        Assert.That(Mathf.Abs(_body.linearVelocity.x), Is.LessThan(0.05f));
    }
}
#endif
