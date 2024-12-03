using AutoMapper;
using TDProjectMVC.Data;
using TDProjectMVC.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TDProjectMVC.Helpers
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<RegisterVM, KhachHang>()
            .ForMember(dest => dest.MaKh, opt => opt.MapFrom(src => src.MaKh))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.HoTen, opt => opt.MapFrom(src => src.HoTen));
            //.ReverseMap();
        }
	}
}
