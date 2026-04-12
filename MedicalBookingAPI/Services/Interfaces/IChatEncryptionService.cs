namespace MedicalBookingAPI.Services.Interfaces;

public interface IChatEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
