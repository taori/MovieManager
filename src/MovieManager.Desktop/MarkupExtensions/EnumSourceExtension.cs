using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace MovieManager.Desktop.MarkupExtensions
{
	public class EnumSourceExtension : MarkupExtension
	{
		public EnumSourceExtension(Type enumType)
		{
			if (enumType == null)
				throw new ArgumentNullException("enumType");

			EnumType = enumType;
		}

		private Type _enumType;
		public Type EnumType
		{
			get { return _enumType; }
			private set
			{
				if (_enumType == value)
					return;

				var enumType = Nullable.GetUnderlyingType(value) ?? value;

				if (enumType.IsEnum == false)
					throw new ArgumentException("Type must be an Enum.");

				_enumType = value;
			}
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var enumValues = Enum.GetValues(EnumType);

			return (
				from object enumValue in enumValues
				select new EnumerationMember
				{
					Value = enumValue,
					Description = GetDescription(enumValue)
				}).ToArray();
		}

		private string GetDescription(object enumValue)
		{
			return EnumType
				.GetField(enumValue.ToString())
				.GetCustomAttributes(typeof(DescriptionAttribute), false)
				.FirstOrDefault() is DescriptionAttribute descriptionAttribute
				? descriptionAttribute.Description
				: enumValue.ToString();
		}

		public struct EnumerationMember
		{
			public string Description { get; set; }
			public object Value { get; set; }
		}
	}
}