using UniRx;
using System;

public class SpeedModel
{
    public ReactiveProperty<float> CurrentSpeed { get; set; }
    public float _maxSpeed { get; set; }

    public SpeedModel(float initialSpeed, float maxSpeed)
    {
        CurrentSpeed = new ReactiveProperty<float>(initialSpeed);
        _maxSpeed = maxSpeed;
    }
    public void Increase(float amount)
    {
        CurrentSpeed.Value = Math.Min(CurrentSpeed.Value + amount, _maxSpeed);
    }
    public void Decrease(float amount)
    {
        CurrentSpeed.Value = Math.Max(CurrentSpeed.Value - amount, 0);
    }
}