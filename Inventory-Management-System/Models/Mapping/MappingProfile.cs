using AutoMapper;
using Inventory_Management_System.BusinessLogic.Encrypt;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.User;

namespace Inventory_Management_System.Models.Mapping
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<UserReqDto, User>()
                .ForMember(dest => dest.UserID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.EncryptedPassword, opt => opt.MapFrom(src => EncryptionHelper.Encrypt(src.Password)));

            CreateMap<User, UserResDto>()
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => EncryptionHelper.Decrypt(src.EncryptedPassword)));
        }
    }
}
