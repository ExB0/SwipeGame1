public class TakeContext
{
    public IColorMatchable CarColor { get; }
    public IColorMatchable PersonColor { get; }
    public IJumpable PersonJump { get; }
    public Car Car { get; }

    public TakeContext(IColorMatchable carColor, IColorMatchable personColor, IJumpable personJump, Car car)
    {
        CarColor = carColor;
        PersonColor = personColor;
        PersonJump = personJump;
        Car = car;
    }
}
