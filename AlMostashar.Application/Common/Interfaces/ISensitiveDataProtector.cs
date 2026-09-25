namespace AlMostashar.Application.Common.Interfaces;

public interface ISensitiveDataProtector
{
    string Protect(string plainText);
    string Unprotect(string protectedText);
    string Mask(string plainText);
}
