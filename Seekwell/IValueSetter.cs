namespace Seekwell
{
    public interface IValueSetter
    {
        void SetValue(object row, object value);
    }

    public class PrimitiveSetter : IValueSetter
    {
        readonly Property _property;

        public PrimitiveSetter(Property property)
        {
            _property = property;
        }

        public void SetValue(object row, object value)
        {
            _property.SetValue(row, value);
        }
    }

    public class ComplexSetter : IValueSetter
    {
        readonly Property _property;
        readonly string _field;

        public ComplexSetter(Property property, string field)
        {
            _property = property;
            _field = field;
        }

        public void SetValue(object row, object value)
        {
            _property.SetChildValue(_property.Name, _field, row, value);
        }
    }

}
