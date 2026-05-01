using AutoMapper;
using WebActionResults.DTOs.Requests;
using WebActionResults.Models;

namespace WebActionResults.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Register, Account>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName.Trim()))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName.Trim()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim()))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone.Trim()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => 1))
            .ForMember(dest => dest.Notes, opt => opt.Ignore());
    }
}