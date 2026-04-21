using MessangersUI.Delegate;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.HasihingPass
{
    public class PasswordhASH
    {
        public ILogger<PasswordhASH> _logger;
        public ExceptionDelegate _exceptionDelegate;

        public PasswordhASH(ILogger<PasswordhASH> logger, ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _exceptionDelegate = exceptionDelegate;
        }

        public async Task<string> Hash(string password)
        {
            try
            {
                using (SHA256 sHA256 = SHA256.Create())
                {
                    byte[] bytes = sHA256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return builder.ToString();
                }
                throw new Exception("Непредвиденное исключение при кэшировании");
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return "";
            }
        }
    }
}
