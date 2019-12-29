using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MathCore.WPF.Commands;

namespace MathCore.WPF
{
    /// <summary>���������������� ������</summary>
    public sealed class UserDialog : FrameworkElement, ICommand
    {
        #region WindowStyle dependency property (Other : ����� ���� �������) : Style

        /// <summary>����� ���� �������</summary>
        public static readonly DependencyProperty WindowStyleProperty =
            DependencyProperty.Register(
                nameof(WindowStyle),
                typeof(Style),
                typeof(UserDialog),
                new PropertyMetadata(default(Style)));

        /// <summary>����� ���� �������</summary>
        [Category("Other")]
        [Description("����� ���� �������")]
        public Style WindowStyle
        {
            get => (Style)GetValue(WindowStyleProperty);
            set => SetValue(WindowStyleProperty, value);
        }

        #endregion

        #region IsDialogDefault dependency property (Other : ����������� �� ��������� � ���� �������) : bool

        /// <summary>����������� �� ��������� � ���� �������</summary>
        public static readonly DependencyProperty IsDialogDefaultProperty =
            DependencyProperty.Register(
                nameof(IsDialogDefault),
                typeof(bool),
                typeof(UserDialog),
                new PropertyMetadata(default(bool)));

        /// <summary>����������� �� ��������� � ���� �������</summary>
        [Category("Other")]
        [Description("����������� �� ��������� � ���� �������")]
        public bool IsDialogDefault
        {
            get => (bool)GetValue(IsDialogDefaultProperty);
            set => SetValue(IsDialogDefaultProperty, value);
        }

        #endregion

        #region WindowContent dependency property (Other : ���������� ����) : object

        /// <summary>���������� ����</summary>
        public static readonly DependencyProperty WindowContentProperty =
            DependencyProperty.Register(
                nameof(WindowContent),
                typeof(object),
                typeof(UserDialog),
                new PropertyMetadata(default(object)));

        /// <summary>���������� ����</summary>
        [Category("Other")]
        [Description("���������� ����")]
        public object WindowContent { get => GetValue(WindowContentProperty); set => SetValue(WindowContentProperty, value); }

        #endregion

        #region Template dependency property (Other : ������ �������) : DataTemplate

        /// <summary>������ �������</summary>
        public static readonly DependencyProperty TemplateProperty =
            DependencyProperty.Register(
                nameof(Template),
                typeof(DataTemplate),
                typeof(UserDialog),
                new PropertyMetadata(default(DataTemplate)));

        /// <summary>������ �������</summary>
        [Category("Other")]
        [Description("������ �������")]
        public DataTemplate Template
        {
            get => (DataTemplate)GetValue(TemplateProperty);
            set => SetValue(TemplateProperty, value);
        }

        #endregion

        #region WindowIcon dependency property (Other : ������ ���� �������) : ImageSource

        /// <summary>������ ���� �������</summary>
        public static readonly DependencyProperty WindowIconProperty =
            DependencyProperty.Register(
                nameof(WindowIcon),
                typeof(ImageSource),
                typeof(UserDialog),
                new PropertyMetadata(default(ImageSource)));

        /// <summary>������ ���� �������</summary>
        [Category("Other")]
        [Description("������ ���� �������")]
        public ImageSource WindowIcon
        {
            get => (ImageSource)GetValue(WindowIconProperty);
            set => SetValue(WindowIconProperty, value);
        }

        #endregion

        #region WindowWidth dependency property (Other : ������ ����) : double

        /// <summary>������ ����</summary>
        public static readonly DependencyProperty WindowWidthProperty =
            DependencyProperty.Register(
                nameof(WindowWidth),
                typeof(double),
                typeof(UserDialog),
                new PropertyMetadata(default(double)));

        /// <summary>������ ����</summary>
        [Category("Other")]
        [Description("������ ����")]
        public double WindowWidth
        {
            get => (double)GetValue(WindowWidthProperty);
            set => SetValue(WindowWidthProperty, value);
        }

        #endregion

        #region WindowHeight dependency property (Other : ������ ����) : double

        /// <summary>������ ����</summary>
        public static readonly DependencyProperty WindowHeightProperty =
            DependencyProperty.Register(
                nameof(WindowHeight),
                typeof(double),
                typeof(UserDialog),
                new PropertyMetadata(default(double)));

        /// <summary>������ ����</summary>
        [Category("Other")]
        [Description("������ ����")]
        public double WindowHeight
        {
            get => (double)GetValue(WindowHeightProperty);
            set => SetValue(WindowHeightProperty, value);
        }

        #endregion

        /// <summary>�������� ���� ��� ����������</summary>
        public ICommand ShowCommand => new LambdaCommand(OnShowCommandExecute);

        /// <summary>�������� ������ � ��������� ������</summary>
        public ICommand ShowDialogCommand => new LambdaCommand(OnShowDialogCommandExecute);

        private void OnShowDialogCommandExecute(object Obj) => CreateWindow(Obj).ShowDialog();

        private void OnShowCommandExecute(object Obj) => CreateWindow(Obj).Show();

        private Window CreateWindow(object Obj)
        {
            var window = new Window();
            window.SetBinding(StyleProperty, new Binding(nameof(WindowStyle)) { Source = this, Mode = BindingMode.OneTime });

            if (Obj != null)
                window.DataContext = Obj;
            else
                window.SetBinding(DataContextProperty, new Binding(nameof(DataContext)) { Source = this });

            var resources = Resources
                .Cast<DictionaryEntry>()
                .Where(r => !(r.Value is Style && ((Style)r.Value).TargetType == typeof(Window) || ((Style)r.Value).TargetType.IsSubclassOf(typeof(Window))));
            foreach (var e in resources) window.Resources.Add(e.Key, e.Value);

            if (WindowContent != null) window.Content = WindowContent;
            else
            {
                var content = new ContentControl();
                content.SetBinding(ContentControl.ContentProperty, new Binding("."));
                if (Template != null) content.SetBinding(ContentControl.ContentTemplateProperty, new Binding(nameof(Template)) { Source = this });
                window.Content = content;
            }

            if (WindowIcon != null) window.SetBinding(Window.IconProperty, new Binding(nameof(WindowIcon)) { Source = this, Mode = BindingMode.OneTime });
            if (ReadLocalValue(WindowHeightProperty) != null) window.SetBinding(HeightProperty, new Binding(nameof(WindowHeight)) { Source = this, Mode = BindingMode.OneTime });
            if (ReadLocalValue(WindowWidthProperty) != null) window.SetBinding(WidthProperty, new Binding(nameof(WindowWidth)) { Source = this, Mode = BindingMode.OneTime });

            return window;
        }

        #region ICommand interface implementation

        /// <inheritdoc />
        public bool CanExecute(object p) => true;

        /// <inheritdoc />
        public void Execute(object p) { if (IsDialogDefault) OnShowDialogCommandExecute(p); else OnShowCommandExecute(p); }

        /// <inheritdoc />
        public event EventHandler CanExecuteChanged { add => CommandManager.RequerySuggested += value; remove => CommandManager.RequerySuggested -= value; }


        #endregion
    }
}