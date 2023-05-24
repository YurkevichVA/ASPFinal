namespace ASPFinal.Data.Entity
{
    public class User
    {
        public Guid         Id                      { get; set; }
        public string       Name                    { get; set; } = null!;
        public string       Login                   { get; set; } = null!;
        public string       PasswordHash            { get; set; } = null!;
        public string       PasswordSalt            { get; set; } = null!;
        public string       Email                   { get; set; } = null!;
        public string?      EmailCode               { get; set; }
        public string?      Avatar                  { get; set; }
        public int          CoinsCount              { get; set; }
        public DateTime     RegistrationDt          { get; set; }
        public DateTime?    DeleteDt                { get; set; }

        /// Navigation properties ///
    }
}
