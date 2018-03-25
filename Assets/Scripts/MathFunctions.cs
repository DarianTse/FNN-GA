using System;

public static class MathFunctions
{
    private static Random random = new Random();

    /// <summary>
    /// Get random number between min and max
    /// </summary>
    /// <param name="min">minimum value</param>
    /// <param name="max">maximum value</param>
    /// <returns>a value between min and max</returns>
    public static double GetRandom(double min, double max)
    {
        if(min > max) { throw new ArgumentException("GetRandom(min,max): Min exceeded Max"); }
        double diff = max - min;
        return min + (random.NextDouble() * diff);
    }

    /// <summary>
    /// Sigmoid function (0-1)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static double Sigmoid(double value)
    {
        return 1 / (1 + Math.Exp(-value));
    }

    /// <summary>
    /// Hyperbolic Tan function (-1 - 1)
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static double TanHFunction(double value)
    {
        return Math.Tanh(value);
    }

    public static float ToDegree(float radians)
    {
        return radians * (180.0f / (float)Math.PI);
    }
}

