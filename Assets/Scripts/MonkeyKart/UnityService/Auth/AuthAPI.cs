using System.Threading.Tasks;
using Unity.Services.Core;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using MonkeyKart.Common;
using System;
using UniRx;

namespace MonkeyKart.UnityService.Lobbies
{
    public static class AuthAPI
    {
        public static string PlayerId
        {
            get
            {
                return AuthenticationService.Instance.PlayerId;
            } 
        }

        public static async UniTask<Result<Unit, string>> InitializeAndSignInAsync()
        {
            try
            {
                await UnityServices.InitializeAsync(new InitializationOptions().SetProfile(Guid.NewGuid().ToString("N")[..30]));
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                return Unit.Default;
            }
            catch (Exception e) {
                return e.Message;
            }
        }
    }
}