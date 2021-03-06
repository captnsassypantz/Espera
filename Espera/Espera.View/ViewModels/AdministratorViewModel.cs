﻿using System;
using System.Windows.Input;
using Espera.Core.Library;
using Rareform.Patterns.MVVM;

namespace Espera.View.ViewModels
{
    public class AdministratorViewModel : ViewModelBase<AdministratorViewModel>
    {
        private readonly Library library;
        private string password;

        public string Password
        {
            get { return this.password; }
            set
            {
                if (this.Password != value)
                {
                    this.password = value;
                    this.OnPropertyChanged(vm => vm.Password);
                }
            }
        }

        public bool IsAdminCreated
        {
            get { return this.library.IsAdministratorCreated; }
        }

        public bool IsAdmin
        {
            get { return this.library.AccessMode == AccessMode.Administrator; }
        }

        public bool IsUser
        {
            get { return this.library.AccessMode == AccessMode.User; }
        }

        public ICommand CreateAdminCommand
        {
            get
            {
                return new RelayCommand
                (
                    param =>
                    {
                        this.library.CreateAdmin(this.Password);
                        this.Password = String.Empty;

                        this.OnPropertyChanged(vm => vm.IsAdminCreated);
                        this.OnPropertyChanged(vm => vm.IsAdmin);
                    },
                    param => !string.IsNullOrWhiteSpace(this.Password)
                );
            }
        }

        public ICommand LoginCommand
        {
            get
            {
                return new RelayCommand
                (
                    param =>
                    {
                        this.library.ChangeToAdmin(this.Password);
                        this.Password = String.Empty;

                        this.OnPropertyChanged(vm => vm.IsAdmin);
                        this.OnPropertyChanged(vm => vm.IsUser);
                    },
                    param => !string.IsNullOrWhiteSpace(this.Password)
                );
            }
        }

        public ICommand ChangeToUserCommand
        {
            get
            {
                return new RelayCommand
                (
                    param =>
                    {
                        this.library.ChangeToUser();
                        this.OnPropertyChanged(vm => vm.IsUser);
                        this.OnPropertyChanged(vm => vm.IsAdmin);
                    }
                );
            }
        }

        public AdministratorViewModel(Library library)
        {
            this.library = library;
        }
    }
}