/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    internal class UserAccountValidation
    {
        public static readonly IValidator UsernameDoesNotContainAtSign =
            new DelegateValidator((service, account, value) =>
            {
                if (value.Contains("@"))
                {
                    Tracing.Verbose("[UserAccountValidation.UsernameDoesNotContainAtSign] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value);

                    return new ValidationResult("Username cannot contain the '@' character.");
                }
                return null;
            });

        public static readonly IValidator UsernameMustNotAlreadyExist =
            new DelegateValidator((service, account, value) =>
            {
                if (service.UsernameExists(account.Tenant, value))
                {
                    Tracing.Verbose("[UserAccountValidation.EmailMustNotAlreadyExist] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value);

                    return new ValidationResult("Username already in use.");
                }
                return null;
            });

        public static readonly IValidator EmailIsValidFormat =
            new DelegateValidator((service, account, value) =>
            {
                EmailAddressAttribute validator = new EmailAddressAttribute();
                if (!validator.IsValid(value))
                {
                    Tracing.Verbose("[UserAccountValidation.EmailIsValidFormat] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value);

                    return new ValidationResult("Email is invalid.");
                }
                return null;
            });

        public static readonly IValidator EmailMustNotAlreadyExist =
            new DelegateValidator((service, account, value) =>
            {
                if (service.EmailExists(account.Tenant, value))
                {
                    Tracing.Verbose("[UserAccountValidation.EmailMustNotAlreadyExist] validation failed: {0}, {1}, {2}", account.Tenant, account.Username, value);
                    
                    return new ValidationResult("Email already in use.");
                }
                return null;
            });

        public static readonly IValidator PasswordMustBeDifferentThanCurrent =
            new DelegateValidator((service, account, value) =>
        {
            // Use LastLogin null-check to see if it's a new account
            // we don't want to run this logic if it's a new account
            if (account.LastLogin != null && account.VerifyHashedPassword(value))
            {
                Tracing.Verbose("[UserAccountValidation.PasswordMustBeDifferentThanCurrent] validation failed: {0}, {1}", account.Tenant, account.Username);

                return new ValidationResult("The new password must be different than the old password.");
            }
            return null;
        });

        public static readonly IValidator PinMustBeNumeric =
            new DelegateValidator((service, account, value) => {
            // Use LastLogin null-check to see if it's a new account
            // we don't want to run this logic if it's a new account
            int result;
            if (!int.TryParse(value, out result)) {
                Tracing.Verbose("[UserAccountValidation.PinMustBeNumeric] validation failed: {0}, {1}", account.Tenant, account.Username);

                return new ValidationResult("The new pin must be numeric.");
            }
            return null;
        });

        public static readonly IValidator PinMustBeCorrectLength =
            new DelegateValidator((service, account, value) => {
            // Use LastLogin null-check to see if it's a new account
            // we don't want to run this logic if it's a new account
            int result;
            if (value.Length != MembershipRebootConstants.UserAccount.StaticPinLength) {
                Tracing.Verbose("[UserAccountValidation.PinMustBeCorrectLength] validation failed: {0}, {1}", account.Tenant, account.Username);

                return new ValidationResult(string.Format("The new pin must be {0} characters in length.", MembershipRebootConstants.UserAccount.StaticPinLength));
            }
            return null;
        });

        public static readonly IValidator PinMustBeDifferentThanCurrent =
            new DelegateValidator((service, account, value) => {
                // Use LastLogin null-check to see if it's a new account
                // we don't want to run this logic if it's a new account
                int result;
                if (account.LastLogin != null && account.TwoFactorStaticPin == value) {
                    Tracing.Verbose("[UserAccountValidation.PinMustBeCorrectLength] validation failed: {0}, {1}", account.Tenant, account.Username);

                    return new ValidationResult("The new pin must be different from the old pin");
                }
                return null;
            });
    }
}
