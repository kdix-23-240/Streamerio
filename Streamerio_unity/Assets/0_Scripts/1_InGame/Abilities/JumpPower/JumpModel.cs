using R3;
using System;

public class JumpModel
{
    public ReactiveProperty<float> CurrentJumpPower { get; set; }
    public float _maxJumpPower { get; set; }

    public JumpModel(float initialJumpPower, float maxJumpPower)
    {
        CurrentJumpPower = new ReactiveProperty<float>(initialJumpPower);
        _maxJumpPower = maxJumpPower;
    }
    public void Increase(float amount)
    {
        CurrentJumpPower.Value = Math.Min(CurrentJumpPower.Value + amount, _maxJumpPower);
    }
    public void Decrease(float amount)
    {
        CurrentJumpPower.Value = Math.Max(CurrentJumpPower.Value - amount, 0);
    }
}