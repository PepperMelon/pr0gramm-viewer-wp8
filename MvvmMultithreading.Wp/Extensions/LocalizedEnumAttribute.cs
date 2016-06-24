namespace Pr0gramm.Extensions
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;
    using System.Windows.Data;
    using ViewModel;

    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly ResourceManager _resource;
        private readonly string _resourceKey;

        public LocalizedDescriptionAttribute(string resourceKey, Type resourceType)
        {
            _resource = new ResourceManager(resourceType);
            _resourceKey = resourceKey;
        }

        public override string Description
        {
            get
            {
                string displayName = _resource.GetString(_resourceKey, new CultureInfo(ViewModelLocator.Settings.AppLang));

                return string.IsNullOrEmpty(displayName)
                    ? string.Format("[[{0}]]", _resourceKey)
                    : displayName;
            }
        }
    }

    public static class EnumExtensions
    {
        public static string GetDescription(this Enum enumValue)
        {
            FieldInfo fi = enumValue.GetType().GetField(enumValue.ToString());

            var attributes =
                (DescriptionAttribute[]) fi.GetCustomAttributes(
                    typeof (DescriptionAttribute),
                    false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            return enumValue.ToString();
        }
    }

    public class LocalizeEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumValue = value as Enum;
            return enumValue.GetDescription();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}