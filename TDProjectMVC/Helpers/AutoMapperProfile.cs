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
			CreateMap<RegisterVM, KhachHang>();
			//.ForMember(kh => kh.HoTen, option => option.MapFrom(RegisterVM => RegisterVM.HoTen))
			//.ReverseMap();
		}
	}
}
