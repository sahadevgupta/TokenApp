using TokenApp.Models;
using TokenApp.Models.Response;

namespace TokenApp.Configurations.Mapper.Converters
{
    public class AdminDtoToAdminUserConverter : ConverterBase<AdminDto, AdminUser>
    {
        protected override AdminUser ConvertImpl(AdminDto source)
        {
            return new AdminUser
            {
                Id = source.Id,
                Username = source.Username ?? string.Empty
            };
        }
    }
}
