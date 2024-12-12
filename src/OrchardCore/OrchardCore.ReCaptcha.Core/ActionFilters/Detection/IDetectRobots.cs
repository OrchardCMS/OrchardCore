namespace OrchardCore.ReCaptcha.ActionFilters.Detection;

/// <summary>
/// This interface describes the contract for components that can detect Robots.
/// </summary>
public interface IDetectRobots
{
    /// <summary>
    /// Performs a check to see if the current request could be submitted by a robot.
    /// </summary>
    /// <returns>Detection result.</returns>
    ValueTask<RobotDetectionResult> DetectRobotAsync(string tag);

    /// <summary>
    /// Clear the detectors internal state, we are not dealing with a robot.
    /// </summary>
    ValueTask IsNotARobotAsync(string tag);

    /// <summary>
    /// We are dealing with a robot, shields up.
    /// </summary>
    ValueTask FlagAsRobotAsync(string tag);
}

public class RobotDetectionResult
{
    public bool IsRobot { get; set; }
}
