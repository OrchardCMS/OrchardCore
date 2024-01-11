namespace OrchardCore.DisplayManagement.Views
{
    public class ShapeViewModel<T> : ShapeViewModel
    {
        public ShapeViewModel(T value)
            : this(typeof(T).Name, value)
        {
        }

        public ShapeViewModel(string shapeType, T value)
            : base(shapeType)
        {
            Value = value;
        }

        public T Value { get; }
    }
}
