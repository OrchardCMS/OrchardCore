namespace OrchardCore.DisplayManagement.Views
{
    public class ShapeViewModel<T> : ShapeViewModel
    {
        public ShapeViewModel(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}
