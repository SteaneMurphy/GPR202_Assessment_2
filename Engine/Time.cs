using System.Diagnostics;

/*
    This class tracks time using a Stopwatch so animation runs at the same speed
    no matter the frame rate.
*/
public class Time
{
    private double previousTime;
    Stopwatch stopWatch;

    /*
        Starts the stopwatch when the Time object is created.
    */
    public Time()
    {
        previousTime = 0f;
        stopWatch = Stopwatch.StartNew();
    }

    /*
        Returns the seconds since the last time this was called (about 0.016 at 60 FPS).
        Used for values that build up each frame, e.g. angle += speed * deltaTime.
        Should only be called once per frame, or the second call returns almost 0.
    */
    public double DeltaTime()
    {
        double elapsedTime = stopWatch.Elapsed.TotalSeconds;
        double deltaTime = elapsedTime - previousTime;
        previousTime = elapsedTime;

        return deltaTime;
    }

    /*
        Returns the seconds since the stopwatch started.
        Used for sine-wave animation, which needs to know where it is in the wave.
    */
    public double TotalTime()
    {
        return stopWatch.Elapsed.TotalSeconds;
    }
}

/*
    References

    Fiedler, G. (2004, June 10). Fix your timestep! Gaffer On Games.
        https://gafferongames.com/post/fix_your_timestep/

    Microsoft. (n.d.). Stopwatch class. Microsoft Learn.
        https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch
*/
