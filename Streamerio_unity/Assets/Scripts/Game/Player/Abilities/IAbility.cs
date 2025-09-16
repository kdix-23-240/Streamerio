using UniRx;

public interface IAbility
{
    ReactiveProperty<float> Amount { get; }

    void Increase(float amount);
    void Decrease(float amount);
}