using ASPFinal.Services.Hash;

namespace ASPFinal.Services.Random
{
    public class RandomServiceV1 : IRandomService
    {
        private readonly string _codeChars = "1234567890qwertyuiopasdfghjklzxcvbnm";
        private readonly string _safeChars = new string(Enumerable.Range(20, 107).Select(x => (char)x).ToArray());
        private readonly System.Random _random = new();
        private readonly IHashService _hashService;

        public RandomServiceV1(IHashService hashService)
        {
            _hashService = hashService;
        }

        public string ConfirmCode(int length)
        {
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = _codeChars[_random.Next(_codeChars.Length)];
            }
            return new string(chars);
        }

        public string RandomFileName()
        {
            string name = RandomString(16);
            name = _hashService.Hash(name + DateTime.Now)[..16];
            return name;

        }

        public string RandomString(int length)
        {
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = _safeChars[_random.Next(_safeChars.Length)];
            }
            return new string(chars);
        }
    }
}
