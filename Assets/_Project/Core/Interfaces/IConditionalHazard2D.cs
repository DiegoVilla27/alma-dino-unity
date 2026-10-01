namespace AlmaDino.Core.Interfaces
{
    public interface IConditionalHazard2D : IHazard2D
    {
        bool IsDangerous { get; }
    }
}
