using System.Diagnostics;

public class Time
{
    private double previousTime;
    Stopwatch stopWatch;
    double totalTime;

    public Time()
    {
        previousTime = 0f;
        totalTime = 0f;
        stopWatch = Stopwatch.StartNew();
    }

    public double DeltaTime()
    {
        double elapsedTime = stopWatch.Elapsed.TotalSeconds;
        double deltaTime = elapsedTime - previousTime;
        previousTime = elapsedTime;

        return deltaTime;
    }

    public double TotalTime()
    {
        return stopWatch.Elapsed.TotalSeconds;
    }
}