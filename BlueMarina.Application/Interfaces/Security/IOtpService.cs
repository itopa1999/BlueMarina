namespace BlueMarina.Application.Interfaces.Security;

public record OtpResult(string RawOtp, string HashedOtp);
public interface IOtpService
{
    OtpResult GenerateOtp(string purpose);
}