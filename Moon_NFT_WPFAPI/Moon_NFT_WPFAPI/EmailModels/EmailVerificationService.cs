namespace Moon_NFT_WPFAPI.EmailModels
{
    public static class EmailVerificationService
    {
        private static readonly Dictionary<string, (string Code, string Email, DateTime Expiry)> _codes = new();

        public static void StoreCode(string tempId, string code, string email, TimeSpan ttl)
        {
            _codes[tempId] = (code, email, DateTime.UtcNow + ttl);
        }

        public static bool TryGetEmailByCode(string tempId, string inputCode, out string email)
        {
            email = string.Empty;

            if (!_codes.TryGetValue(tempId, out var data))
                return false;

            if (DateTime.UtcNow > data.Expiry)
            {
                _codes.Remove(tempId);
                return false;
            }

            if (data.Code == inputCode)
            {
                email = data.Email;
                _codes.Remove(tempId); // одноразовый
                return true;
            }

            return false;
        }
    }
}
