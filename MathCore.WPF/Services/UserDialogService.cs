﻿using System;
using System.IO;
using System.Linq;
using System.Windows;

using MathCore.WPF.ViewModels;
using MathCore.WPF.Views.Windows;

using Microsoft.Win32;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace MathCore.WPF.Services
{
    /// <summary>Сервис диалога с пользователем</summary>
    public class UserDialogService : IUserDialogBase
    {
        /// <summary>Активное окно приложения</summary>
        protected static Window? ActiveWindow => Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive);

        /// <summary>Окно с фокусом ввода</summary>
        protected static Window? FocusedWindow => Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsFocused);

        /// <summary>Текущее окно приложения</summary>
        protected static Window? CurrentWindow => FocusedWindow ?? ActiveWindow;

        /// <summary>Открыть диалога выбора файла для чтения</summary>
        /// <param name="Title">Заголовок диалогового окна</param>
        /// <param name="Filter">Фильтр файлов диалога</param>
        /// <param name="DefaultFilePath">Путь к файлу по умолчанию</param>
        /// <returns>Выбранный файл, либо null, если диалог был отменён</returns>
        public virtual FileInfo? OpenFile(string Title, string Filter = "Все файлы (*.*)|*.*", string? DefaultFilePath = null)
        {
            var dialog = new OpenFileDialog
            {
                Title = Title,
                RestoreDirectory = true,
                Filter = Filter ?? throw new ArgumentNullException(nameof(Filter)),
            };
            if (DefaultFilePath is { Length: > 0 })
                dialog.FileName = DefaultFilePath;

            return dialog.ShowDialog(CurrentWindow) == true
                ? new(dialog.FileName)
                : DefaultFilePath is null ? null : new(DefaultFilePath);
        }

        /// <summary>Открыть диалога выбора файла для записи</summary>
        /// <param name="Title">Заголовок диалогового окна</param>
        /// <param name="Filter">Фильтр файлов диалога</param>
        /// <param name="DefaultFilePath">Путь к файлу по умолчанию</param>
        /// <returns>Выбранный файл, либо null, если диалог был отменён</returns>
        public virtual FileInfo? SaveFile(string Title, string Filter = "Все файлы (*.*)|*.*", string? DefaultFilePath = null)
        {
            var dialog = new SaveFileDialog
            {
                Title = Title,
                RestoreDirectory = true,
                Filter = Filter ?? throw new ArgumentNullException(nameof(Filter)),
            };
            if (DefaultFilePath is { Length: > 0 })
                dialog.FileName = DefaultFilePath;

            return dialog.ShowDialog(CurrentWindow) == true
                ? new(dialog.FileName)
                : DefaultFilePath is null ? null : new(DefaultFilePath);
        }

        /// <summary>Диалог с текстовым вопросом и вариантами выбора Yes/No</summary>
        /// <param name="Text">Заголовок окна диалога</param>
        /// <param name="Title">Текст в окне диалога</param>
        /// <returns>Истина, если был сделан выбор Yes</returns>
        public virtual bool YesNoQuestion(string Text, string Title = "Вопрос...")
        {
            var result = CurrentWindow is null
                ? MessageBox.Show(Text, Title, MessageBoxButton.YesNo, MessageBoxImage.Question)
                : MessageBox.Show(CurrentWindow, Text, Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        /// <summary>Диалог с текстовым вопросом и вариантами выбора Ok/Cancel</summary>
        /// <param name="Text">Заголовок окна диалога</param>
        /// <param name="Title">Текст в окне диалога</param>
        /// <returns>Истина, если был сделан выбор Ok</returns>
        public virtual bool OkCancelQuestion(string Text, string Title = "Вопрос...")
        {
            var result = CurrentWindow is null
                ? MessageBox.Show(Text, Title, MessageBoxButton.OKCancel, MessageBoxImage.Question)
                : MessageBox.Show(CurrentWindow, Text, Title, MessageBoxButton.OKCancel, MessageBoxImage.Question);
            return result == MessageBoxResult.OK;
        }

        /// <summary>Диалог с информацией</summary>
        /// <param name="Text">Заголовок окна диалога</param>
        /// <param name="Title">Текст в окне диалога</param>
        public virtual void Information(string Text, string Title = "Сообщение...")
        {
            if (CurrentWindow is null)
                MessageBox.Show(Text, Title, MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(CurrentWindow, Text, Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>Диалог с предупреждением</summary>
        /// <param name="Text">Заголовок окна диалога</param>
        /// <param name="Title">Текст в окне диалога</param>
        public virtual void Warning(string Text, string Title = "Предупреждение!")
        {
            if (CurrentWindow is null)
                MessageBox.Show(Text, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            else
                MessageBox.Show(CurrentWindow, Text, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>Диалог с ошибкой</summary>
        /// <param name="Text">Заголовок окна диалога</param>
        /// <param name="Title">Текст в окне диалога</param>
        public virtual void Error(string Text, string Title = "Ошибка!")
        {
            if (CurrentWindow is null)
                MessageBox.Show(Text, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show(CurrentWindow, Text, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>Диалог индикации прогресса операции</summary>
        /// <param name="Title">Заголовок диалога</param>
        /// <param name="Status">Текст сообщения в диалоге</param>
        /// <param name="Information">Информационное сообщение в окне диалога</param>
        /// <returns>Объект управления диалогом</returns>
        /// <example>
        /// <code>
        /// using(var dialog = dialog_service.Progress)
        /// {
        ///     await OperationAsync(dialog.Progress, dialog.Cancel);
        /// }
        /// </code>
        /// </example>
        public virtual IProgressInfo Progress(string Title, string Status, string? Information = null)
        {
            var progress_model = new ProgressViewModel(Title)
            {
                StatusValue = Status,
                InformationValue = Information
            };
            var progress_view = new ProgressDialogWindow
            {
                DataContext = progress_model,
                Owner = CurrentWindow
            };
            void OnDisposed(object? s, EventArgs e)
            {
                if (progress_view.Dispatcher.CheckAccess())
                    progress_view.Close();
                else
                    progress_view.Dispatcher.Invoke(() => OnDisposed(s, e));
            }

            progress_model.Disposed += OnDisposed;
            progress_view.Closing += (s, e) => e.Cancel = ((Window)s).DataContext is ProgressViewModel
            {
                IsDisposed: false,
                Cancellable: false,
            };
            progress_view.Closed += (_, _) => progress_model.CancelCommand.Execute(default);
            progress_view.Show();

            return progress_model;
        }

        /// <summary>Запрос ввода текста</summary>
        /// <param name="Caption">Текст в окне диалога</param>
        /// <param name="Title">Текст в заголовке окна диалога</param>
        /// <param name="Default">Текстовое значение по умолчанию</param>
        /// <returns>Введённый текст, либо null в случае отказа</returns>
        public string? GetText(string Caption, string Title = "Введите текст", string? Default = "")
        {
            var view_model = new DialogTextViewModel
            {
                Title = Title,
                Caption = Caption,
                Value = Default
            };
            var view = new TextRequestDialogWindow
            {
                DataContext = view_model,
                Owner = CurrentWindow,
            };

            void OnCompleted(object? s, EventArgs<bool?> e)
            {
                if (!view.Dispatcher.CheckAccess())
                    view.Dispatcher.Invoke(() => OnCompleted(s, e));
                else
                {
                    view.DialogResult = e;
                    view.Close();
                }
            }

            view_model.Completed += OnCompleted;

            return view.ShowDialog() == true
                ? view_model.Value
                : null;
        }
    }
}