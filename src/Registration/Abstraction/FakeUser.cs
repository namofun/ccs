using Microsoft.AspNetCore.Identity;
using System;
using Xylab.Contesting.Entities;

namespace Xylab.Contesting.Registration
{
    public class FakeRegisterUser : IUser
    {
        public int Id { get; }

        public FakeRegisterUser(int userid) => Id = userid;

        DateTimeOffset? IUser.LockoutEnd { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool IUser.TwoFactorEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool IUser.PhoneNumberConfirmed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUser.PhoneNumber { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUser.ConcurrencyStamp => throw new NotImplementedException();
        string IUser.SecurityStamp => throw new NotImplementedException();
        bool IUser.EmailConfirmed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUser.Email { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUser.UserName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool IUser.LockoutEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        int IUser.AccessFailedCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUser.NickName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUser.Plan { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        DateTimeOffset? IUser.RegisterTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool IUser.SubscribeNews { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool IUser.HasPassword() => throw new NotImplementedException();
        bool IUser.HasUserName(string username) => throw new NotImplementedException();
    }

    public class FakeRegisterUserWithRating : FakeRegisterUser, IUserWithRating
    {
        public int? Rating { get; set; }

        public FakeRegisterUserWithRating(int uid, int? rating) : base(uid)
        {
            Rating = rating;
        }
    }
}
