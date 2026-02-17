using FluentValidation;
using OAuthServer.V2.Core.DTOs.User;

namespace OAuthServer.V2.Service.Validations;

public class SignUpRequestValidator : AbstractValidator<SignUpRequest>
{
    // NOT: .NET PIPELINE SENKRON OLARAK ÇALIŞIR. BU YÜZDEN BUNUN İÇİNDE VERİTABANINDA GİDİP KONTROL YAPACAKSAK
    // VE BUNU PIPELINE İLE BİRLİKTE ÇALIŞTIRACAKSAK BUNUN SENKRON ÇALIŞACAĞINI VE PERFORMANS KAYBINA SEBEBİYET
    // VERECEĞİNİ BİLMEMİZ GEREKİR.

    // Business Validation + .NET Pipeline => SENKRON
    // Business Validation +  IValidator<> => ASENKRON

    // BURADA User TABLOSUNA KENDİ EKLEDİĞİMİZ ALANLARIN VALİDASYONU YAPILIR. DEFAULT GELEN ALANLAR IDENTITY API
    // KONFİGÜRASYONU İÇERİSİNDE GERÇEKLEŞTİRİLİR.

    public SignUpRequestValidator()
    {
        // AT LEAST ONE OF EMAIL OR PHONENUMBER MUST BE PROVIDED
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email) || !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Email or phone number is required.");

        // FIRSTNAME VALIDATION
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters.");

        // EMAIL VALIDATION (WHEN PROVIDED)
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        // PHONE NUMBER VALIDATION (WHEN PROVIDED) - E.164 FORMAT
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+[1-9]\d{6,14}$").WithMessage("Phone number must be in E.164 format (e.g., +905551234567).")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        // PASSWORD VALIDATION - IDENTITY API HANDLES DEFAULT RULES BUT WE ADD EXTRA
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters.");

        // BIRTHDATE VALIDATION
        RuleFor(x => x.BirthDate)
            .LessThan(DateTime.Now.AddYears(-13)).WithMessage("User must be at least 13 years old.")
            .GreaterThan(DateTime.Now.AddYears(-120)).WithMessage("Invalid birth date.");

        // USERNAME VALIDATION (WHEN PROVIDED)
        RuleFor(x => x.UserName)
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters.")
            .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("Username can only contain letters, numbers, dots, hyphens, and underscores.")
            .When(x => !string.IsNullOrWhiteSpace(x.UserName));
    }
}