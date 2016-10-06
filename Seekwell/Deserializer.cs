using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Seekwell
{
	public class Deserializer : IDeserializer
	{
		static Dictionary<Type, Property[]> typeCache = new Dictionary<Type, Property[]>();

		public IEnumerable<T> Deserialize<T>(IDataReader reader) where T : new()
		{
			var result = new List<T>();
			var properties = GetProperties<T>();

			Dictionary<int, IValueSetter> map = new Dictionary<int, IValueSetter>();
			for (int i = 0; i < reader.FieldCount; i++)
			{
				var columnName = reader.GetName(i);
				var property = GetMatchingProperty(properties, columnName);
				if (property != null) map.Add(i, property);
			}

			while (reader.Read())
			{
				var row = new T();

				foreach (var item in map)
				{
					var value = reader[item.Key];
					if (value != DBNull.Value)
					{
						item.Value.SetValue(row, value);
					}
				}

				result.Add(row);
			}
			return result;
		}

        static IValueSetter GetMatchingProperty(Property[] properties, string name)
        {
            var propertyName = name;
            var fieldName = name;

            if (name.Contains("."))
            {
                propertyName = name.Substring(0, name.IndexOf("."));
                fieldName = name.Replace(propertyName + ".", "");
            }

            foreach (var property in properties)
            {
                if (property.Name.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (propertyName == name) return new PrimitiveSetter(property);
                    return new ComplexSetter(property, fieldName);
                }
            }
            return null;
        }


        static Property[] GetProperties<T>()
		{
			Property[] result;
			if (typeCache.TryGetValue(typeof(T), out result))
			{
				return result;
			}

			var properties = ConvertPropertyInfo(typeof(T));

			try
			{
				typeCache.Add(typeof(T), properties);
			}
			catch (Exception) { }

			return properties;
		}

		static Property[] ConvertPropertyInfo(Type type)
		{
			var result = new List<Property>();
			var properties = type.GetProperties();
			foreach (var property in properties)
			{
				result.Add(new Property(property));
			}
			return result.ToArray();
		}
	}


    public class Property
    {
        delegate object Converter(object value, Type targetType);

        readonly PropertyInfo _property;
        readonly Converter convert;
        readonly List<Property> children = new List<Property>();

        public Property(PropertyInfo property)
        {
            _property = property;

            if (property.PropertyType.Module.Name != "mscorlib.dll")
            {
                foreach (var child in property.PropertyType.GetProperties())
                {
                    children.Add(new Property(child));
                }
            }

            this.Name = property.Name;
            this.Type = property.PropertyType.IsGenericType ? property.PropertyType.GetGenericArguments()[0] : property.PropertyType;

            convert = property.PropertyType.IsEnum ? (Converter)this.ConvertStringToEnum : (Converter)Convert.ChangeType;
        }

        public Type Type { get; private set; }
        public string Name { get; private set; }

        public void SetValue(object obj, object value)
        {
            object typedValue = convert(value, this.Type);
            _property.SetValue(obj, typedValue, null);
        }

        public void SetChildValue(string propertyName, string field, object obj, object value)
        {
            foreach (var child in children)
            {
                if (child.Name == field)
                {
                    var property = obj.GetType().GetProperty(propertyName);
                    var propVal = property.GetValue(obj, null);
                    if (propVal == null)
                    {
                        propVal = property.PropertyType.GetConstructor(Type.EmptyTypes).Invoke(null);
                        property.SetValue(obj, propVal, null);
                    }
                    child.SetValue(propVal, value);
                }
            }
        }

        object ConvertStringToEnum(object value, Type targetType)
        {
            string valueAsString = value.ToString();
            return Enum.Parse(targetType, valueAsString, true);
        }
    }
}
