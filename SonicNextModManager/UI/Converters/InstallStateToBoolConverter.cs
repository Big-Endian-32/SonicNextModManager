﻿namespace SonicNextModManager.UI.Converters
{
    [ValueConversion(typeof(InstallState), typeof(bool))]
    public class InstallStateToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is InstallState out_state)
            {
                // Treat InstallState.Uninstalling the same as InstallState.Installing for this context.
                if ((out_state == InstallState.Installing || out_state == InstallState.Uninstalling) &&
                    (value.Equals(InstallState.Installing) || value.Equals(InstallState.Uninstalling)))
                {
                    return true;
                }
            }

            return value.Equals(parameter ?? InstallState.Idle) ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
