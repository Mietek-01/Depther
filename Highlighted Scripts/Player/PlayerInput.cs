using System;
using UnityEngine;

[DefaultExecutionOrder(-10)]// Before the Player.cs and the PlayerDashMoveCreator.cs
public partial class PlayerInput : MonoBehaviour, IPlayerInputData, IPlayerInputManagement
{
    [SerializeField] ButtonInput jump = new ButtonInput(KeyCode.W, "Jump");
    [SerializeField] ButtonInput crouch = new ButtonInput(KeyCode.S, "Crouch");

    [SerializeField] MouseInput shot = new MouseInput(0, "Shot");

    [SerializeField] AxisInput horizontalMovement = new AxisInput("Horizontal", "HorizontalMovement");

    [SerializeField] DoubleTapInput leftDoubleTap = new DoubleTapInput(KeyCode.A, "LeftDashMove");
    [SerializeField] DoubleTapInput rightDoubleTap = new DoubleTapInput(KeyCode.D, "RightDashMove");
    [SerializeField] DoubleTapInput upperDoubleTap = new DoubleTapInput(KeyCode.W, "UpperDashMove");

    public bool Jump => jump.Result;

    public bool Shot => shot.Result;

    public bool ReleasedShot => shot.Released;

    public bool Crouch => crouch.Result;

    public bool StandUp => crouch.Released;

    public bool LeftDoubleTap => leftDoubleTap.Result;

    public bool RightDoubleTap => rightDoubleTap.Result;

    public bool UpperDoubleTap => upperDoubleTap.Result;

    public float HorizontalMovement => horizontalMovement.Result;

    public bool Enabled
    {
        get => enabled;
        set
        {
            enabled = value;

            if (value == false)
                foreach (var input in myInputs)
                    input.Reset();
        }
    }

    InputData[] myInputs;

    private void Awake()
    {
        myInputs = new InputData[] { jump, crouch, shot, horizontalMovement, leftDoubleTap, rightDoubleTap, upperDoubleTap };
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var input in myInputs)
            if (input.Enabled)
                input.Update();
    }

    public void EnableInputFor(string actionInputName, bool value)
    {
        var result = Array.Find(myInputs, input => input.ForAction == actionInputName);

        if (result != null)
            result.Enabled = value;
        else
            Debug.LogError($"I dont have an input action named {actionInputName}");
    }
}
