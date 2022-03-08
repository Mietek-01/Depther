public interface IPlayerInputData
{
    bool Jump { get; }

    bool Shot { get; }

    bool ReleasedShot { get; }

    bool Crouch { get; }

    bool StandUp { get; }

    bool LeftDoubleTap { get; }

    bool RightDoubleTap { get; }

    bool UpperDoubleTap { get; }

    float HorizontalMovement { get; }
}
