﻿using System;
using System.Windows.Input;

using MathCore.WPF.Commands;
// ReSharper disable EventNeverSubscribedTo.Global

namespace MathCore.WPF.ViewModels
{
    public class DialogViewModel : TitledViewModel
    {
        public event EventHandler<EventArgs<bool?>>? Completed;

        protected virtual void OnCompleted(bool? Result) => Completed?.Invoke(this, Result);

        #region Command CompletedCommand : bool? - Команда завершения диалога

        /// <summary>Команда завершения диалога</summary>
        private Command? _CompletedCommand;

        /// <summary>Команда завершения диалога</summary>
        public ICommand CompletedCommand => _CompletedCommand ??= Command.New<bool?>(OnCompletedCommandExecuted, CanCompletedCommandExecute);

        /// <summary>Проверка возможности выполнения - Команда завершения диалога</summary>
        protected virtual bool CanCompletedCommandExecute(bool? p) => true;

        /// <summary>Логика выполнения - Команда завершения диалога</summary>
        protected virtual void OnCompletedCommandExecuted(bool? p) => OnCompleted(p);

        #endregion

        #region Command CommitCommand - Команда Commit

        /// <summary>Команда Commit</summary>
        private Command? _CommitCommand;

        /// <summary>Команда Commit</summary>
        public ICommand CommitCommand => _CommitCommand ??= Command.New(OnCommitCommandExecuted, CanCommitCommandExecute);

        /// <summary>Проверка возможности выполнения - Команда Commit</summary>
        protected virtual bool CanCommitCommandExecute() => true;

        /// <summary>Логика выполнения - Команда Commit</summary>
        protected virtual void OnCommitCommandExecuted() => OnCompleted(true);

        #endregion

        #region Command CancelCommand - Команда Cancel

        /// <summary>Команда Cancel</summary>
        private Command? _CancelCommand;

        /// <summary>Команда Cancel</summary>
        public ICommand CancelCommand => _CancelCommand ??= Command.New(OnCancelCommandExecuted, CanCancelCommandExecute);

        /// <summary>Проверка возможности выполнения - Команда Cancel</summary>
        protected virtual bool CanCancelCommandExecute() => true;

        /// <summary>Логика выполнения - Команда Cancel</summary>
        protected virtual void OnCancelCommandExecuted() => OnCompleted(false);

        #endregion
    }
}