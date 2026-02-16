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
        
    }
}