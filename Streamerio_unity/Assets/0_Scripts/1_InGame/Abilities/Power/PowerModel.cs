using R3;
using System;

public class PowerModel
{
    public ReactiveProperty<float> CurrentPower { get; set; }
    public float _maxPower { get; set; }

    public PowerModel(float initialPower, float maxPower)
    {
        CurrentPower = new ReactiveProperty<float>(initialPower);
        _maxPower = maxPower;
    }
    public void Increase(float amount)
    {
        CurrentPower.Value = Math.Min(CurrentPower.Value + amount, _maxPower);
    }
    public void Decrease(float amount)
    {
        CurrentPower.Value = Math.Max(CurrentPower.Value - amount, 0);
    }
}