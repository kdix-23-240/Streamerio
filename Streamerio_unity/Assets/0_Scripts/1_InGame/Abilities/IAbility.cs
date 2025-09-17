using R3;

public interface IAbility
{
   float Amount { get; }

    void Increase(float amount);
    void Decrease(float amount);
}