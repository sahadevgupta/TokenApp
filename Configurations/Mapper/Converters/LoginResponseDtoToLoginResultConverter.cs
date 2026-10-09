using TokenApp.Models;
using TokenApp.Models.Response;

namespace TokenApp.Configurations.Mapper.Converters
{
    public class LoginResponseDtoToLoginResultConverter : ConverterBase<LoginResponseDto, LoginResult>
    {
        protected override LoginResult ConvertImpl(LoginResponseDto source)
        {
            return new LoginResult
            {
                Token = source.Token ?? string.Empty,
                Admin = BackendToAppModelMapper.GetAdmin(source.Admin)
            };
        }
    }
}
