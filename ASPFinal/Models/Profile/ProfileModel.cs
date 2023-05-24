namespace ASPFinal.Models.Profile
{
    public class ProfileModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsNamePublic { get; set; }
        public string Email { get; set; } = null!;
        public bool IsEmailPublic { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public string Login { get; set; } = null!;
        public string? Avatar { get; set; }
        public DateTime RegistrationDt { get; set; }
        public DateTime? LastEnter { get; set; }
        public bool IsDateTimesPublic { get; set; }
        /// <summary>
        /// Чи належить модель автентифікованого користувача
        /// </summary>
        public bool IsPersonal { get; set; }
        public ProfileModel(Data.Entity.User user)
        {
            var thisProps = this.GetType().GetProperties();
            foreach (var prop in user.GetType().GetProperties())
            {
                var thisProp = thisProps.FirstOrDefault(p =>
                    p.Name == prop.Name
                    && p.PropertyType.IsAssignableFrom(prop.PropertyType));

                thisProp?.SetValue(this, prop.GetValue(user));
            }
            this.IsEmailConfirmed = user.EmailCode is null;
        }
    }
}
