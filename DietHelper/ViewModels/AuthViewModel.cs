using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietHelper.Common.DTO;
using DietHelper.Services;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.ViewModels
{
    public partial class AuthViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        //private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string userName = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private string message = string.Empty;

        [ObservableProperty]
        private bool isNewUser = false;

        public AuthViewModel(ApiService apiService)
        {
            _apiService = apiService;
            //_navigationService = navigationService;

            //if (_apiService.IsAuthenticated)
            //_ = _navigationService.NavigateToMainAsync();

            Message = "Авторизуйтесь в системе";
        }

        [RelayCommand]
        private async Task Login()
        {
            if (IsNewUser) return;

            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
                Message = "Введите логин и пароль";

            try
            {
                var result = await _apiService.LoginAsync(
                    new LoginDto
                    {
                        UserName = UserName,
                        Password = Password
                    });
                if (result!.IsSuccess)
                {
                    Message = "Авторизация прошла успешно";
                    //await _navigationService.NavigateToMainAsync();
                }
                else Message = "Неверный логин или пароль";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthViewModel]: {ex.Message}");
                Message = "Ошибка авторизации";
            }
        }

        private bool IsPasswordValid()
        {
            if (Password.Length >= 8 && Password.Length <= 50 && Password.Any(char.IsDigit)
                && Password.Any(char.IsLower) && Password.Any(char.IsUpper))
                return true;
            return false;
        }

        [RelayCommand]
        private async Task Register()
        {
            if (!IsNewUser) return;

            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
                Message = "Введите логин и пароль";

            if (!IsPasswordValid())
                Message = "Пароль должен быть не менее 8 символов и не более 50, содержать хотя бы одну строчную и одну прописную букву, а также цифру";

            if (string.IsNullOrWhiteSpace(ConfirmPassword)) Message = "Подтвердите пароль";

            if (Password != ConfirmPassword) Message = "Пароли не совпадают";

            try
            {
                var result = await _apiService.RegisterAsync(
                    new RegisterDto
                    {
                        UserName = UserName,
                        Password = Password
                    });
                if (result!.IsSuccess)
                {
                    Message = "Регистрация прошла успешно";
                    //await _navigationService.NavigateToLoginAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthViewModel]: {ex.Message}");
                Message = "Ошибка регистрации";
            }
        }

        [RelayCommand]
        private void Reset()
        {
            UserName = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            Message = "Авторизуйтесь в системе";
        }
    }
}
