using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;


public class InputActionBasedFirstPersonControllerInput : FirstPersonControllerInput
{
    private FirstPersonInputAction _controls;
    private float lookSmoothingFactor = 14.0f;

    protected void Awake()
    {
        _controls = new FirstPersonInputAction();
        //Hide Mouse Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //Move
        _move = this.UpdateAsObservable()
            .Select(_ =>
            {
                return _controls.Player.Move.ReadValue<Vector2>();
            });

        //Look
        var smoothLookValue = new Vector2(0,0);
        _look = this.UpdateAsObservable()
            .Select(_ =>
            {
                var rawLookValue = _controls.Player.Look.ReadValue<Vector2>();
                smoothLookValue = new Vector2(
                    Mathf.Lerp(smoothLookValue.x, rawLookValue.x, lookSmoothingFactor * Time.deltaTime),
                    Mathf.Lerp(smoothLookValue.y, rawLookValue.y, lookSmoothingFactor * Time.deltaTime)
                );
                return smoothLookValue;
            });

        //Run
        _run = this.UpdateAsObservable()
                .Select(_ => _controls.Player.Run.ReadValueAsObject() != null)
                .ToReadOnlyReactiveProperty();

        //Jump
        _jump = new Subject<Unit>().AddTo(this);
        _controls.Player.Jump.performed += context => {
            _jump.OnNext(Unit.Default);
        };

        //Go Next
        _goNext = new Subject<Unit>().AddTo(this);
        _controls.Player.GoNext.performed += context => {
            _goNext.OnNext(Unit.Default);
        };
    }

    private void OnEnable()
    {
        _controls.Enable();
    }

    private void OnDisable()
    {
        _controls.Disable();
    }


    private IObservable<Vector2> _move;
    public override IObservable<Vector2> Move
    {
        get { return _move; }
    }
    private Subject<Unit> _jump;
    public override IObservable<Unit> Jump
    {
        get { return _jump; }
    }

    private Subject<Unit> _goNext;
    public override IObservable<Unit> GoNext
    {
        get { return _goNext; }
    }

    private ReadOnlyReactiveProperty<bool> _run;
    public override ReadOnlyReactiveProperty<bool> Run
    {
        get { return _run; }
    }
    private IObservable<Vector2> _look;
    public override IObservable<Vector2> Look
    {
        get { return _look; }
    }
}