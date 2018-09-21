namespace rrServiceNetV2.Common
{
    public interface ICrypter
    {
        string Decrypt(byte[] data, int bytesRead);
    }
}
