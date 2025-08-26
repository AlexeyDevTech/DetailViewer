using System.Windows;
using System.Windows.Controls;

namespace DetailViewer.Modules.Dialogs
{
    /// <summary>
    /// Вспомогательный класс для привязки свойства PasswordBox к ViewModel.
    /// </summary>
    public static class PasswordBoxHelper
    {
        /// <summary>
        /// Присоединенное свойство для привязки пароля.
        /// </summary>
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxHelper), new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        /// <summary>
        /// Присоединенное свойство для отслеживания обновления пароля.
        /// </summary>
        private static readonly DependencyProperty UpdatingPasswordProperty =
            DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false));

        /// <summary>
        /// Получает значение присоединенного свойства BoundPassword.
        /// </summary>
        /// <param name="d">Объект зависимости.</param>
        /// <returns>Привязанный пароль.</returns>
        public static string GetBoundPassword(DependencyObject d)
        {
            return (string)d.GetValue(BoundPasswordProperty);
        }

        /// <summary>
        /// Устанавливает значение присоединенного свойства BoundPassword.
        /// </summary>
        /// <param name="d">Объект зависимости.</param>
        /// <param name="value">Значение пароля.</param>
        public static void SetBoundPassword(DependencyObject d, string value)
        {
            d.SetValue(BoundPasswordProperty, value);
        }

        /// <summary>
        /// Получает значение присоединенного свойства UpdatingPassword.
        /// </summary>
        /// <param name="d">Объект зависимости.</param>
        /// <returns>True, если пароль обновляется, иначе false.</returns>
        private static bool GetUpdatingPassword(DependencyObject d)
        {
            return (bool)d.GetValue(UpdatingPasswordProperty);
        }

        /// <summary>
        /// Устанавливает значение присоединенного свойства UpdatingPassword.
        /// </summary>
        /// <param name="d">Объект зависимости.</param>
        /// <param name="value">Значение флага обновления.</param>
        private static void SetUpdatingPassword(DependencyObject d, bool value)
        {
            d.SetValue(UpdatingPasswordProperty, value);
        }

        /// <summary>
        /// Обработчик изменения привязанного пароля.
        /// </summary>
        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox box)
            {
                box.PasswordChanged -= HandlePasswordChanged;
                string newPassword = (string)e.NewValue;
                if (!GetUpdatingPassword(box))
                {
                    box.Password = newPassword;
                }
                box.PasswordChanged += HandlePasswordChanged;
            }
        }

        /// <summary>
        /// Обработчик события изменения пароля в PasswordBox.
        /// </summary>
        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox box)
            {
                SetUpdatingPassword(box, true);
                SetBoundPassword(box, box.Password);
                SetUpdatingPassword(box, false);
            }
        }
    }
}