using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        private readonly IRepository<MobileOtp> _otpRepository;
        private readonly IRepository<User> _userRepository;
        private readonly ISmsService _smsService;
        private readonly IConfiguration _configuration;

        public OtpService(
            IRepository<MobileOtp> otpRepository,
            IRepository<User> userRepository,
            ISmsService smsService,
            IConfiguration configuration)
        {
            _otpRepository = otpRepository;
            _userRepository = userRepository;
            _smsService = smsService;
            _configuration = configuration;
        }

        public async Task SendOtpAsync(string mobileNumber)
        {
            // Deactivate existing
            var existing = await _otpRepository.Query()
                .Where(o => o.PhoneNumber == mobileNumber && o.IsActive)
                .ToListAsync();

            foreach (var o in existing)
            {
                o.IsActive = false;
                await _otpRepository.UpdateAsync(o);
            }

            var code = GenerateSixDigitCode();
            var otp = new MobileOtp
            {
                PhoneNumber = mobileNumber,
                Code = code,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsVerified = false
            };

            await _otpRepository.AddAsync(otp);
            await _smsService.SendSmsAsync(mobileNumber, $"OTP Code: {code}");
        }

        public async Task<bool> VerifyOtpAsync(string mobileNumber, string code, int userId)
        {
            if (!int.TryParse(code, out var codeInt)) return false;

            var otp = await _otpRepository.Query()
                .FirstOrDefaultAsync(o => o.PhoneNumber == mobileNumber && o.Code == codeInt && o.IsActive);

            if (otp == null) return false;

            var expirationSeconds = int.Parse(_configuration["Otp:SmsExpirationSeconds"] ?? "300");
            if (otp.CreatedAt.AddSeconds(expirationSeconds) < DateTime.UtcNow)
            {
                otp.IsActive = false;
                await _otpRepository.UpdateAsync(otp);
                return false;
            }

            otp.IsVerified = true;
            otp.IsActive = false;
            await _otpRepository.UpdateAsync(otp);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.MobileVerified = true;
                await _userRepository.UpdateAsync(user);
            }

            return true;
        }

        public int GenerateSixDigitCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000);
        }
    }
}
